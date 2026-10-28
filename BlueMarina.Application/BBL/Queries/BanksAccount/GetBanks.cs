using System.Net;
using System.ComponentModel.DataAnnotations;
using MediatR;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Queries.BanksAccount;

public sealed class GetBanks
{
    public sealed class Query : IRequest<BaseResult<GetBanksDto>>
    {
        [MaxLength(100)]
        public string? Name { get; init; }
    }

    public sealed class GetBanksDto
    {
        public class BankDto
        {
            public string Name { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public string? Logo { get; set; }
        }
        public List<BankDto> Banks { get; init; } = [];
    }

    public sealed class Handler : IRequestHandler<Query, BaseResult<GetBanksDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<GetBanksDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Query<Domain.Entities.Home.Bank>()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var search = request.Name.Trim().ToLowerInvariant();
                query = query.Where(x => x.Name.ToLower().Contains(search));
            }

            var banks = await query
                .OrderBy(x => x.Name)
                .Select(x => new GetBanksDto.BankDto
                {
                    Name = x.Name,
                    Code = x.Code,
                    Logo = x.Logo
                })
                .ToListAsync(cancellationToken);

            return new BaseResult<GetBanksDto>(
                HttpStatusCode.OK,
                "Bank data retrieved successfully.",
                new GetBanksDto
                {
                    Banks = banks
                });
        }
    }
}