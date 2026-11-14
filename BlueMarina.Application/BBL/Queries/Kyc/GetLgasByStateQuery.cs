using MediatR;
using System.Net;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Shared.Common.Results;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BlueMarina.Application.BBL.Queries.Kyc;

public sealed class GetLgasByStateQuery
{
    public class Query : IRequest<BaseResult<List<LgaDto>>>
    {
        [JsonIgnore]
        public long StateId { get; set; }
    }

    public class LgaDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Query, BaseResult<List<LgaDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<List<LgaDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.StateId <= 0)
                return new BaseResult<List<LgaDto>>(HttpStatusCode.BadRequest, "Invalid State ID.");

            var lgas = await _unitOfWork.Query<Domain.Entities.Onboarding.Lga>()
                .AsNoTracking()
                .Where(x => x.StateId == request.StateId)
                .OrderBy(x => x.Name)
                .Select(x => new LgaDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);

            return new BaseResult<List<LgaDto>>(
                HttpStatusCode.OK, 
                "LGAs fetched successfully.", 
                lgas);
        }
    }
}