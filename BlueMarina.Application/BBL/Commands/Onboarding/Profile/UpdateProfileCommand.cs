// using System.Net;
// using System.ComponentModel.DataAnnotations;
// using System.Text.Json.Serialization;
// using BlueMarina.Application.Interfaces.Persistence;
// using BlueMarina.Domain.Entities;
// using BlueMarina.Shared.Results;
// using MediatR;
// using Microsoft.EntityFrameworkCore;

// namespace BlueMarina.Application.BBL.Commands.Profile;

// public sealed class UpdateProfileCommand
// {
//     public class Command : IRequest<BaseResult>
//     {
//         [JsonIgnore]
//         public string UserId { get; set; } = string.Empty;

//         [MaxLength(100)]
//         [Required]
//         public string? FullName { get; set; }
//         public DateTime? DateOfBirth { get; set; }

//         public string? Gender { get; set; }

//         public string? MaritalStatus { get; set; }

//         [MaxLength(50)]
//         public string? Nationality { get; set; }
//     }

//     public class Handler : IRequestHandler<Command, BaseResult>
//     {
//         private readonly IUnitOfWork _unitOfWork;

//         public Handler(IUnitOfWork unitOfWork)
//         {
//             _unitOfWork = unitOfWork;
//         }

//         public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
//         {
//             var userProfile = await _unitOfWork.Query<UserProfile>()
//                 .Where(up => up.Id == request.UserId && !up.IsDeleted)
//                 .FirstOrDefaultAsync(cancellationToken);

//             if (userProfile == null)
//             {
//                 return new BaseResult(HttpStatusCode.NotFound, "User profile not found.");
//             }

//             if (!string.IsNullOrWhiteSpace(request.FullName))
//             {
//                 var nameParts = request.FullName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
//                 if (nameParts.Length < 2)
//                 {
//                     return new BaseResult(HttpStatusCode.BadRequest, "Full name must contain both a first name and a last name.");
//                 }
                
//                 var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
//                 userProfile.FirstName = textInfo.ToTitleCase(nameParts[0]);
//                 userProfile.LastName = textInfo.ToTitleCase(nameParts[^1]);
//                 userProfile.OtherNames = nameParts.Length > 2 ? string.Join(" ", nameParts[1..^1].Select(textInfo.ToTitleCase)) : null;
//             }

//             if (request.DateOfBirth.HasValue)
//             {
//                 if (request.DateOfBirth.Value > DateTime.UtcNow.AddYears(-13))
//                 {
//                     return new BaseResult(HttpStatusCode.BadRequest, "User must be at least 13 years old.");
//                 }
//                 userProfile.DateOfBirth = request.DateOfBirth;
//             }

//             if (!string.IsNullOrWhiteSpace(request.Gender))
//             {
//                 var cleanedGender = request.Gender.Trim().ToLowerInvariant();
//                 switch (cleanedGender)
//                 {
//                     case "male":
//                         userProfile.Gender = Gender.Male;
//                         break;
//                     case "female":
//                         userProfile.Gender = Gender.Female;
//                         break;
//                     case "other":
//                         userProfile.Gender = Gender.Other;
//                         break;
//                     default:
//                         return new BaseResult(HttpStatusCode.BadRequest, 
//                             $"'{request.Gender}' is not a valid Gender. Allowed values are: 'Male', 'Female', or 'Other'.");
//                 }
//             }

//             if (!string.IsNullOrWhiteSpace(request.MaritalStatus))
//             {
//                 var cleanedMarital = request.MaritalStatus.Trim().ToLowerInvariant();
//                 switch (cleanedMarital)
//                 {
//                     case "single":
//                         userProfile.MaritalStatus = MaritalStatus.Single;
//                         break;
//                     case "married":
//                         userProfile.MaritalStatus = MaritalStatus.Married;
//                         break;
//                     case "divorced":
//                         userProfile.MaritalStatus = MaritalStatus.Divorced;
//                         break;
//                     case "widowed":
//                         userProfile.MaritalStatus = MaritalStatus.Widowed;
//                         break;
//                     default:
//                         return new BaseResult(HttpStatusCode.BadRequest, 
//                             $"'{request.MaritalStatus}' is not a valid Marital Status. Allowed values are: 'Single', 'Married', 'Divorced', or 'Widowed'.");
//                 }
//             }

//             userProfile.Nationality = request.Nationality ?? userProfile.Nationality;

//             _unitOfWork.Update(userProfile);
//             await _unitOfWork.SaveChangesAsync(cancellationToken);

//             return new BaseResult(HttpStatusCode.OK, "User profile updated successfully.");
//         }
//     }
// }