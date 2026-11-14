using MediatR;
using System.Net;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Shared.Common.Results;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BlueMarina.Application.BBL.Queries.Kyc;

public sealed class GetStatesQuery
{
    public class Query : IRequest<BaseResult<List<StateDto>>>
    {
    }

    public class StateDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Query, BaseResult<List<StateDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<List<StateDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var states = await _unitOfWork.Query<Domain.Entities.Onboarding.State>()
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new StateDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);

            return new BaseResult<List<StateDto>>(
                HttpStatusCode.OK, 
                "States fetched successfully.", 
                states);
        }
    }
}
