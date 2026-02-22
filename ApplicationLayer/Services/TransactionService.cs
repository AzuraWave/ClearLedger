using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Query;
using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.DTOs.Transactions.Adjustment;
using ApplicationLayer.DTOs.Transactions.Discount;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.DTOs.Transactions.Payments;
using ApplicationLayer.DTOs.Transactions.Query;
using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IDiscountRepository _discountRepo;
        private readonly IAdjustmentRepository _adjustmentRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IClientRepository _clientRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IBalanceService _balanceService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            IInvoiceRepository invoiceRepo,
            IDiscountRepository discountRepo,
            IAdjustmentRepository adjustmentRepo,
            IPaymentRepository paymentRepo,
            IClientRepository clientRepo,
            IProjectRepository projectRepo,
            IBalanceService balanceService,
            IUnitOfWork unitOfWork,
            ILogger<TransactionService> logger)
        {
            _invoiceRepo = invoiceRepo;
            _discountRepo = discountRepo;
            _adjustmentRepo = adjustmentRepo;
            _paymentRepo = paymentRepo;
            _clientRepo = clientRepo;
            _projectRepo = projectRepo;
            _balanceService = balanceService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }


        public async Task<Guid> CreateInvoiceAsync(CreateInvoiceDto dto, Guid organizationId, Guid createdByUserId)
        {
            _logger.LogInformation("Creating invoice {@OrgId} for project {@ProjectId} by user {@UserId}", organizationId, dto.ProjectId, createdByUserId);

            var project = await _projectRepo.GetByIdAsync(dto.ProjectId, organizationId);
            if (project == null)
            {
                _logger.LogWarning("Project not found: {@ProjectId}", dto.ProjectId);
                throw new Exception("Project not found");
            }

            var client = await _clientRepo.GetByIdAsync(project.ClientId, organizationId);
            if (client == null)
            {
                _logger.LogWarning("Client not found: {@ClientId}", project.ClientId);
                throw new Exception("Client not found");
            }

            if (!dto.Lines.Any())
            {
                _logger.LogWarning("Invoice must have at least one line for project {@ProjectId}", project.Id);
                throw new Exception("Invoice must have at least one line");
            }

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ClientId = client.Id,
                ProjectId = project.Id,
                InvoiceNumber = await _invoiceRepo.GetNextInvoiceNumberAsync(organizationId, client.Id),
                Date = dto.Date,
                Reference = dto.Reference,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId,
                Lines = dto.Lines.Select(l => new InvoiceLine
                {
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.Quantity * l.UnitPrice,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                }).ToList()
            };

            invoice.TotalAmount = invoice.Lines.Sum(l => l.LineTotal);

            await _invoiceRepo.AddAsync(invoice);

            _logger.LogInformation("Adjusting balances for invoice {@InvoiceId} with total {@TotalAmount}", invoice.Id, invoice.TotalAmount);

            await _balanceService.AdjustProjectBalanceAsync(project.Id, invoice.TotalAmount, organizationId, true);
            await _balanceService.AdjustClientBalanceAsync(client.Id, invoice.TotalAmount, organizationId, true);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Invoice created successfully: {@InvoiceId}", invoice.Id);

            return invoice.Id;
        }

        public async Task<Guid> CreateProjectPaymentAsync(
            CreateProjectPaymentDto dto,
            Guid organizationId,
            Guid createdByUserId)
        {
            _logger.LogInformation("Creating project payment {@ProjectId} for {@OrgId} by user {@UserId}, amount {@Amount}", dto.ProjectId, organizationId, createdByUserId, dto.Amount);


            var project = await _projectRepo.GetByIdAsync(dto.ProjectId, organizationId);
            if (project == null)
            {
                _logger.LogWarning("Project not found: {@ProjectId}", dto.ProjectId);
                throw new Exception("Project not found");
            }

            var client = await _clientRepo.GetByIdAsync(project.ClientId, organizationId);
            if (client == null)
            {
                _logger.LogWarning("Client not found: {@ClientId}", project.ClientId);
                throw new Exception("Client not found");
            }

            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Payment amount must be positive, provided: {@Amount}", dto.Amount);
                throw new Exception("Amount must be positive");
            }
            
            var payment = new ClientPaymentHeader
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ClientId = client.Id,
                TotalAmount = dto.Amount,
                Date = dto.Date,
                Reference = dto.Reference,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId,
                Allocations = new List<ClientPaymentAllocation>
        {
            new ClientPaymentAllocation
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Amount = dto.Amount
            }
        }
            };

            await _paymentRepo.AddAsync(payment);

            await _balanceService.AdjustProjectBalanceAsync(project.Id, dto.Amount, organizationId, false);
            await _balanceService.AdjustClientBalanceAsync(client.Id, dto.Amount, organizationId, false);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Project payment created successfully {@PaymentId} for project {@ProjectId}", payment.Id, project.Id);

            return payment.Id;
        }


        public async Task<Guid> CreateClientPaymentAsync(
            CreateClientPaymentDto dto,
            Guid organizationId,
            Guid createdByUserId)
        {
            _logger.LogInformation("Creating client payment {@ClientId} for {@OrgId} by user {@UserId}, total {@TotalAmount}",
        dto.ClientId, organizationId, createdByUserId, dto.TotalAmount);

            if (dto.TotalAmount <= 0)
            {
                _logger.LogWarning("Invalid payment amount {@TotalAmount} for client {@ClientId}", dto.TotalAmount, dto.ClientId);
                throw new Exception("Total amount must be positive");
            }

            if (dto.Allocations == null || !dto.Allocations.Any())
            {
                _logger.LogWarning("No allocations provided for client payment {@ClientId}", dto.ClientId);
                throw new Exception("At least one allocation is required");
            }

            var totalAllocated = dto.Allocations.Sum(a => a.Amount);
            if (totalAllocated != dto.TotalAmount)
            {
                _logger.LogWarning("Allocation total {@Allocated} does not match payment total {@Total} for client {@ClientId}",
                    totalAllocated, dto.TotalAmount, dto.ClientId);
                throw new Exception("Allocation total must equal payment total");
            }

            var client = await _clientRepo.GetByIdAsync(dto.ClientId, organizationId);
            if (client == null)
            {
                _logger.LogWarning("Client not found {@ClientId}", dto.ClientId);
                throw new Exception("Client not found");
            }

            var payment = new ClientPaymentHeader
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ClientId = dto.ClientId,
                TotalAmount = dto.TotalAmount,
                Date = dto.Date,
                Reference = dto.Reference,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId,
                Allocations = new List<ClientPaymentAllocation>()
            };

            foreach (var alloc in dto.Allocations)
            {
                if (alloc.Amount <= 0)
                {
                    _logger.LogWarning("Invalid allocation amount {@Amount} for project {@ProjectId}", alloc.Amount, alloc.ProjectId);
                    throw new Exception("Allocation amount must be positive");
                }

                var project = await _projectRepo.GetByIdAsync(alloc.ProjectId, organizationId);
                if (project == null || project.ClientId != dto.ClientId)
                {
                    _logger.LogWarning("Invalid project {@ProjectId} for client {@ClientId}", alloc.ProjectId, dto.ClientId);
                    throw new Exception("Invalid project for this client");
                }

                payment.Allocations.Add(new ClientPaymentAllocation
                {
                    Id = Guid.NewGuid(),
                    ProjectId = alloc.ProjectId,
                    Amount = alloc.Amount
                });

                await _balanceService.AdjustProjectBalanceAsync(alloc.ProjectId, alloc.Amount, organizationId, false);
                await _balanceService.AdjustClientBalanceAsync(dto.ClientId, alloc.Amount, organizationId, false);
            }

            await _paymentRepo.AddAsync(payment);

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Client payment created {@PaymentId} for client {@ClientId}", payment.Id, dto.ClientId);

            return payment.Id;
        }

        

        public async Task<Guid> ApplyDiscountAsync(
            CreateDiscountDto dto,
            Guid organizationId,
            Guid createdByUserId)
        {
            _logger.LogInformation("Applying discount {@Amount} for project {@ProjectId}, client {@ClientId}, org {@OrgId}, user {@UserId}",
        dto.Amount, dto.ProjectId, dto.ClientId, organizationId, createdByUserId);

            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Invalid discount amount {@Amount} for project {@ProjectId}", dto.Amount, dto.ProjectId);
                throw new Exception("Discount amount must be positive");
            }

            var project = await _projectRepo.GetByIdAsync(dto.ProjectId, organizationId);
            if (project == null)
            {
                _logger.LogWarning("Project not found {@ProjectId}", dto.ProjectId);
                throw new Exception("Project not found");
            }

            var client = await _clientRepo.GetByIdAsync(project.ClientId, organizationId);
            if (client == null)
            {
                _logger.LogWarning("Client not found {@ClientId}", project.ClientId);
                throw new Exception("Client not found");
            }

            var discount = new Discount
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ClientId = client.Id,
                ProjectId = dto.ProjectId,
                Amount = dto.Amount,
                Date = dto.Date,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId,
            };

            await _discountRepo.AddAsync(discount);

            // Reduces balance
            await _balanceService.AdjustProjectBalanceAsync(project.Id, dto.Amount, organizationId, false);
            await _balanceService.AdjustClientBalanceAsync(client.Id, dto.Amount, organizationId, false);

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Discount applied {@DiscountId} for project {@ProjectId}", discount.Id, project.Id);


            return discount.Id;
        }

        // Adjustment

        public async Task<Guid> CreateAdjustmentAsync(
    CreateAdjustmentDto dto,
    Guid organizationId,
    Guid createdByUserId)
        {
            _logger.LogInformation("Creating adjustment {@Amount} for project {@ProjectId}, client {@ClientId}, org {@OrgId}, user {@UserId}, positive {@IsPositive}",
       dto.Amount, dto.ProjectId, dto.ClientId, organizationId, createdByUserId, dto.IsPositive);

            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Invalid adjustment amount {@Amount} for client {@ClientId}", dto.Amount, dto.ClientId);
                throw new Exception("Adjustment amount must be positive");
            }

            var project = await _projectRepo.GetByIdAsync(dto.ProjectId, organizationId);
            if (project == null)
            {
                _logger.LogWarning("Project not found {@ProjectId}", dto.ProjectId);
                throw new Exception("Project not found");
            }

            var client = await _clientRepo.GetByIdAsync(project.ClientId, organizationId);
            if (client == null)
            {
                _logger.LogWarning("Client not found {@ClientId}", project.ClientId);
                throw new Exception("Client not found");
            }

            var adjustment = new Adjustment
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ClientId = dto.ClientId,
                ProjectId = dto.ProjectId,
                Amount = dto.Amount,
                IsPositive = dto.IsPositive,
                Date = dto.Date,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId,
            };

            await _adjustmentRepo.AddAsync(adjustment);

            if (dto.IsPositive)
            {
                await _balanceService.AdjustProjectBalanceAsync(
                    dto.ProjectId,
                    dto.Amount,
                    organizationId,
                    true);
                await _balanceService.AdjustClientBalanceAsync(
                    client.Id,
                    dto.Amount,
                    organizationId,
                    true);
            }
            else
            {
                await _balanceService.AdjustProjectBalanceAsync(
                    dto.ProjectId,
                    dto.Amount,
                    organizationId,
                    false);
                await _balanceService.AdjustClientBalanceAsync(
                    client.Id,
                    dto.Amount,
                    organizationId,
                    false);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Adjustment created {@AdjustmentId} for project {@ProjectId}, client {@ClientId}",
        adjustment.Id, dto.ProjectId, dto.ClientId);
            return adjustment.Id;
        }

        // Voiding

        public async Task VoidInvoiceAsync(
            Guid invoiceId,
            Guid organizationId,
            Guid voidedByUserId)
        {
            _logger.LogInformation("Voiding invoice {@InvoiceId} for org {@OrgId} by user {@UserId}", invoiceId, organizationId, voidedByUserId);

            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId, organizationId);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found {@InvoiceId}", invoiceId);
                throw new Exception("Invoice not found");
            }

            if (invoice.IsVoided)
            {
                _logger.LogWarning("Invoice already voided {@InvoiceId}", invoiceId);
                throw new Exception("Invoice already voided");
            }

            var project = await _projectRepo.GetByIdAsync(invoice.ProjectId, organizationId);
            if (project == null)
            {
                _logger.LogWarning("Project not found {@ProjectId}", invoice.ProjectId);
                throw new Exception("Project not found");
            }

            var client = await _clientRepo.GetByIdAsync(project.ClientId, organizationId);
            if (client == null)
            {
                _logger.LogWarning("Client not found {@ClientId}", project.ClientId);
                throw new Exception("Client not found");
            }

            invoice.IsVoided = true;
            invoice.VoidedAt = DateTime.UtcNow;

            await _balanceService.AdjustProjectBalanceAsync(project.Id, invoice.TotalAmount, organizationId, false);
            await _balanceService.AdjustClientBalanceAsync(client.Id, invoice.TotalAmount, organizationId, false);

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Invoice {@InvoiceId} voided successfully", invoiceId);
        }

        public async Task VoidPaymentAsync(
            Guid paymentId,
            Guid organizationId,
            Guid userId)
        {
            _logger.LogInformation("Voiding payment {@PaymentId} for org {@OrgId} by user {@UserId}", paymentId, organizationId, userId);

            var payment = await _paymentRepo.GetByIdAsync(paymentId, organizationId);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found {@PaymentId}", paymentId);
                throw new Exception("Payment not found");
            }

            if (payment.IsVoided)
            {
                _logger.LogWarning("Payment already voided {@PaymentId}", paymentId);
                throw new Exception("Payment already voided");
            }

            var client = await _clientRepo.GetByIdAsync(payment.ClientId, organizationId);
            if (client == null)
            {
                _logger.LogWarning("Client not found {@ClientId}", payment.ClientId);
                throw new Exception("Client not found");
            }


            payment.IsVoided = true;
            payment.VoidedAt = DateTime.UtcNow;

            foreach (var alloc in payment.Allocations)
            {
                await _balanceService.AdjustProjectBalanceAsync(alloc.ProjectId, alloc.Amount, organizationId, true);
                await _balanceService.AdjustClientBalanceAsync(client.Id, alloc.Amount, organizationId, true);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Payment {@PaymentId} voided successfully", paymentId);
        }



        // READS

        public async Task<AdjustmentReadDto?> GetAdjustmentDetailsAsync(Guid adjustmentId, Guid organizationId)
        {
            _logger.LogInformation(
    "Fetching adjustment details. adjustmentId {adjustmentId}, OrgId {OrgId}",
    adjustmentId, organizationId);
            var adjustment = await _adjustmentRepo.GetByIdAsync(adjustmentId, organizationId);
            if (adjustment == null) {
                _logger.LogWarning(
    "adjustment not found. adjustmentId {adjustmentId}, OrgId {OrgId}",
    adjustmentId, organizationId);
                return null; }

            return new AdjustmentReadDto
            {
                Id = adjustment.Id,
                Date = adjustment.Date,
                ClientId = adjustment.ClientId,
                ProjectId = adjustment.ProjectId,
                Amount = adjustment.Amount,
                Reference = adjustment.Reason
            };
        }

        public async Task<PaymentReadDto?> GetClientPaymentDetailsAsync(Guid paymentId, Guid organizationId)
        {
            _logger.LogInformation(
            "Fetching payment details. paymentId {paymentId}, OrgId {OrgId}",
            paymentId, organizationId);

            var payment = await _paymentRepo.GetByIdAsync(paymentId, organizationId);
            if (payment == null) {
                _logger.LogWarning(
    "payment not found. paymentId {paymentId}, OrgId {OrgId}",
    paymentId, organizationId); 
                return null; }

            return new PaymentReadDto
            {
                Id = payment.Id,
                Date = payment.Date,
                ClientId = payment.ClientId,
                Reference = payment.Reference,
                TotalAmount = payment.TotalAmount,
                Allocations = payment.Allocations.Select(a => new PaymentAllocationDto
                {
                    ProjectId = a.ProjectId,
                    Amount = a.Amount
                }).ToList()
            };
        }

        public async Task<DiscountReadDto?> GetDiscountDetailsAsync(Guid discountId, Guid organizationId)
        {
            _logger.LogInformation(
            "Fetching discount details. discountId {discountId}, OrgId {OrgId}",
            discountId, organizationId);
            var discount = await _discountRepo.GetByIdAsync(discountId, organizationId);
            if (discount == null) {
                _logger.LogWarning(
    "discount not found. discountId {discountId}, OrgId {OrgId}",
    discountId, organizationId);
                return null; }

            return new DiscountReadDto
            {
                Id = discount.Id,
                Date = discount.Date,
                ClientId = discount.ClientId,
                ProjectId = discount.ProjectId,
                Amount = discount.Amount,
                Reference = discount.Reason
            };
        }

        public async Task<InvoiceReadDto?> GetInvoiceDetailsAsync(Guid invoiceId, Guid organizationId)
        {
            _logger.LogInformation(
                "Fetching invoice details. InvoiceId {InvoiceId}, OrgId {OrgId}",
                invoiceId, organizationId);
            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId, organizationId);
            if (invoice == null) {
                _logger.LogWarning(
    "Invoice not found. InvoiceId {InvoiceId}, OrgId {OrgId}",
    invoiceId, organizationId);
                return null; }

            return new InvoiceReadDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                Date = invoice.Date,
                ClientId = invoice.ClientId,
                ClientName = invoice.Client.Name,
                ProjectId = invoice.ProjectId,
                ProjectName = invoice.Project.Name,
                Reference = invoice.Reference,
                TotalAmount = invoice.TotalAmount,
                Lines = invoice.Lines.Select(l => new InvoiceLineDto
                {
                    Id = l.Id,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal
                }).ToList()
            };
        }

        public virtual async Task<PagedResult<TransactionDto>> SearchTransactionsAsync(TransactionQueryDto dto)
        {
            _logger.LogInformation(
    "Searching transactions. OrgId {OrgId}, ClientId {ClientId}, Page {Page}, PageSize {PageSize}",
    dto.OrganizationId, dto.ClientId, dto.Page, dto.PageSize);
            var invoiceQuery = _invoiceRepo.QueryAll(dto.OrganizationId)
            .Where(i => !i.IsVoided)
            .Select(i => new TransactionDto
            {
                Id = i.Id,
                Type = TransactionType.Invoice,
                Date = i.Date,
                ClientId = i.ClientId,
                ClientName = i.Client.Name,
                ProjectId = i.ProjectId,
                ProjectName = i.Project.Name,
                Amount = i.Lines.Sum(line => line.LineTotal), 
                Reference = i.Reference
            })
            .AsQueryable();

            var discountQuery = _discountRepo.QueryAll(dto.OrganizationId)
                .Select(d => new TransactionDto
                {
                    Id = d.Id,
                    Type = TransactionType.Discount,
                    Date = d.Date,
                    ClientId = d.ClientId,
                    ClientName = d.Client.Name,
                    ProjectId = d.ProjectId,
                    ProjectName = d.Project.Name,
                    Amount = -d.Amount,  
                    Reference = d.Reason
                });

            var adjustmentQuery = _adjustmentRepo.QueryAll(dto.OrganizationId)
                .Select(a => new TransactionDto
                {
                    Id = a.Id,
                    Type = TransactionType.Adjustment,
                    Date = a.Date,
                    ClientId = a.ClientId  ,
                    ClientName = a.Client.Name,
                    ProjectId = a.ProjectId,
                    ProjectName = a.Project.Name,
                    Amount = a.IsPositive ? a.Amount : -a.Amount,  
                    Reference = a.Reason
                });

            var paymentQuery = _paymentRepo.QueryAll(dto.OrganizationId)
                .Where(p => !p.IsVoided)
                .SelectMany(p => p.Allocations, (p, alloc) => new TransactionDto
                {
                    Id = p.Id,
                    Type = TransactionType.Payment,
                    Date = p.Date,
                    ClientId = p.ClientId,
                    ClientName = p.Client.Name,
                    ProjectId = alloc.ProjectId,
                    ProjectName = alloc.Project.Name,
                    Amount = -alloc.Amount,
                    Reference = p.Reference
                });

            var allTransactions = invoiceQuery
                .Concat(discountQuery)
                .Concat(adjustmentQuery)
                .Concat(paymentQuery);

            // filtering, sorting, paging
            if (dto.ClientId.HasValue)
                allTransactions = allTransactions.Where(t => t.ClientId == dto.ClientId.Value);

            if (dto.ProjectId.HasValue)
                allTransactions = allTransactions.Where(t => t.ProjectId == dto.ProjectId.Value);

            if (!string.IsNullOrWhiteSpace(dto.Search))
                allTransactions = allTransactions.Where(t => t.Reference!.Contains(dto.Search));

            if (dto.From.HasValue)
                allTransactions = allTransactions.Where(t => t.Date >= dto.From.Value);

            if (dto.To.HasValue)
                allTransactions = allTransactions.Where(t => t.Date <= dto.To.Value);
            if(dto.MinAmount.HasValue)
                allTransactions = allTransactions.Where(t => t.Amount >= dto.MinAmount.Value);
            if (dto.MaxAmount.HasValue)
                allTransactions = allTransactions.Where(t => t.Amount <= dto.MaxAmount.Value);

            allTransactions = dto.SortBy switch
            {
                "DateAsc" => allTransactions.OrderBy(t => t.Date),
                "DateDesc" => allTransactions.OrderByDescending(t => t.Date),
                "AmountAsc" => allTransactions.OrderBy(t => t.Amount),
                "AmountDesc" => allTransactions.OrderByDescending(t => t.Amount),
                "TypeAsc" => allTransactions.OrderBy(t => t.Type),
                "TypeDesc" => allTransactions.OrderByDescending(t => t.Type),
                "ReferenceAsc" => allTransactions.OrderBy(t => t.Reference),
                "ReferenceDesc" => allTransactions.OrderByDescending(t => t.Reference),
                _ => allTransactions.OrderByDescending(t => t.Date)
            };

            
            var totalCount = await allTransactions.CountAsync();

            var items = await allTransactions
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            _logger.LogInformation(
    "Transactions search completed. OrgId {OrgId}, TotalCount {TotalCount}",
    dto.OrganizationId, totalCount);
            return new PagedResult<TransactionDto>(items, totalCount);


        }

        public async Task<decimal> GetOrgInvoicesForCurrentMonth(Guid organizationId)
        {
            return await _invoiceRepo.GetOrgInvoicesForCurrentMonth(organizationId);
        }


        public async Task<ClientOverviewDto> GetClientOverviewAsync(Guid organizationId, Guid clientId)
        {
            _logger.LogInformation(
    "Generating client overview. OrgId {OrgId}, ClientId {ClientId}",
    organizationId, clientId);
            ClientOverviewDto Result = new ClientOverviewDto();

            var invoicesQuery =  _invoiceRepo.QueryByClient(organizationId, clientId);

            var totalInvoiced = await invoicesQuery
                .Where(i => !i.IsVoided)
                .SumAsync(i => i.Lines.Sum(l => l.LineTotal));

            Result.TotalInvoiced = totalInvoiced;

            var paymentQuery = _paymentRepo.QueryByClient(organizationId, clientId);

            var totalPaid = await paymentQuery.Where(i => !i.IsVoided)
                .SumAsync(i => i.Allocations.Sum(l => l.Amount));

            Result.TotalPaid = totalPaid;

            var projectsQuery = await _projectRepo.QueryAll(organizationId);
            var projects = await projectsQuery.Where(i => i.ClientId == clientId && i.Status == ProjectStatus.Active).CountAsync();
               
                

            Result.ActiveProjects = projects;
            _logger.LogInformation(
    "Generated client overview. OrgId {OrgId}, ClientId {ClientId}",
    organizationId, clientId);
            return Result;
        }

       

        public async Task<StatementOfAccountDto> GetStatementAsync(Guid orgId, Guid clientId, DateTime from, DateTime to)
        {

            _logger.LogInformation(
    "Generating statement. OrgId {OrgId}, ClientId {ClientId}, From {From}, To {To}",
    orgId, clientId, from, to);
            var openingTransactions = await SearchTransactionsAsync(new TransactionQueryDto
            {
                ClientId = clientId,
                OrganizationId = orgId,
                To = from.Date.AddDays(-1)
            });

            var openingBalance = openingTransactions.Items.Sum(t => t.Amount);


            var periodTransactions = (await SearchTransactionsAsync(new TransactionQueryDto
            {
                ClientId= clientId,
                OrganizationId= orgId,
                From = from,
                To = to,
                SortBy = "DateAsc"
            })).Items.ToList();

            var Client = await _clientRepo.GetByIdAsync(clientId);
            if (Client == null)
            {
                throw new Exception("Client not found");
            }

            decimal running = openingBalance;
            foreach (var tx in periodTransactions)
            {
                running += tx.Amount;
                tx.RunningBalance = running;
            }

            _logger.LogInformation(
            "Statement generated. OrgId {OrgId}, ClientId {ClientId}, TxCount {Count}",
            orgId, clientId, periodTransactions.Count);
            return new StatementOfAccountDto
            {
                ClientId = clientId,
                ClientName = Client.Name ?? "Client",
                From = from,
                To = to,
                OpeningBalance = openingBalance,
                Transactions = periodTransactions
            };
        }
    }
}
