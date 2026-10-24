
// using System.Net;
// using System.Text.Json.Serialization;
// using BlueMarina.Application.Interfaces.Persistence;
// using BlueMarina.Domain.Entities;
// using BlueMarina.Shared.Results;
// using MediatR;
// using Microsoft.EntityFrameworkCore;


// namespace BlueMarina.Application.BBL.Queries.Kyc;
// public sealed class GetKycStatusQuery
// {
//     public class Query : IRequest<BaseResult<KycStatusDto>>
//     {
//         [JsonIgnore] public long UserId { get; set; }
//     }

//     public class KycStatusDto
//     {
//         public string CurrentLevel { get; set; }
//         public int CurrentTier { get; set; } = 1;
//         public bool HasBvn { get; set; }
//         public bool HasNin { get; set; }
//         public bool HasValidIdDocument { get; set; }
//     }

//     public class Handler : IRequestHandler<Query, BaseResult<KycStatusDto>>
//     {
//         private readonly IUnitOfWork _unitOfWork;

//         public Handler(IUnitOfWork unitOfWork)
//         {
//             _unitOfWork = unitOfWork;
//         }

//         public async Task<BaseResult<KycStatusDto>> Handle(Query request, CancellationToken cancellationToken)
//         {
//             var kycProfile = await _unitOfWork.Query<KycProfile>()
//                 .AsNoTracking()
//                 .FirstOrDefaultAsync(x => x.UserProfileId == request.UserId, cancellationToken);

//             if (kycProfile is null)
//             {
//                 return new BaseResult<KycStatusDto>(
//                     HttpStatusCode.OK,
//                     "User is at the default KYC level.",
//                     new KycStatusDto
//                     {
//                         CurrentLevel = nameof(KycLevel.Basic),
//                         HasBvn = false,
//                         HasNin = false,
//                         HasValidIdDocument = false
//                     });
//             }

//             var hasBvn = !string.IsNullOrWhiteSpace(kycProfile.Bvn);
//             var hasNin = !string.IsNullOrWhiteSpace(kycProfile.Nin);

//             int currentTier = kycProfile.KycLevel switch
//             {
//                 KycLevel.Basic => 1,
//                 KycLevel.Intermediate => 2,
//                 KycLevel.Advanced => 3,
//                 _ => 1
//             };

//             return new BaseResult<KycStatusDto>(
//                 HttpStatusCode.OK,
//                 "KYC status retrieved successfully.",
//                 new KycStatusDto
//                 {
//                     CurrentLevel = nameof(kycProfile.KycLevel),
//                     CurrentTier = currentTier,
//                     HasBvn = hasBvn,
//                     HasNin = hasNin,
//                     HasValidIdDocument = hasBvn || hasNin
//                 });
//         }
//     }
// }