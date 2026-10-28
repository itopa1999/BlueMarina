using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Home;
using BlueMarina.Shared.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Queries.BanksAccount;

public sealed class GetAllBankAccount
{
    public class Query : IRequest<BaseResult<List<GetAllBankAccountDto>>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
    }

    public class GetAllBankAccountDto
    {
        public long Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankImageUrl { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public sealed class Handler : IRequestHandler<Query, BaseResult<List<GetAllBankAccountDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<List<GetAllBankAccountDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return new BaseResult<List<GetAllBankAccountDto>>(
                    HttpStatusCode.BadRequest,
                    "User public ID is required.");
            }

            var accounts = await _unitOfWork.Query<BankAccount>()
                .AsNoTracking()
                .Where(x => x.UserId == request.UserId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new GetAllBankAccountDto
                {
                    Id = x.Id,
                    AccountNumber = x.AccountNumber,
                    AccountName = x.AccountName,
                    BankName = x.BankName,
                    BankImageUrl = x.BankImageUrl,
                    IsDefault = x.IsDefault
                })
                .ToListAsync(cancellationToken);

            return new BaseResult<List<GetAllBankAccountDto>>(HttpStatusCode.OK, "Banks Account retrieve successfully", accounts);
        }
    }
}