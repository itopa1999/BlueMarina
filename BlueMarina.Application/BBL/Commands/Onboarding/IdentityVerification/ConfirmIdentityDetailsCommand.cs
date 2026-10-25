// public sealed class ConfirmIdentityDetailsCommand
// {
//     public class Command : IRequest<BaseResult>
//     {
//         [Required] public string Bvn { get; set; } = string.Empty;
//         // Pass the details back from FE to ensure they match what FE showed
//         [Required] public string FirstName { get; set; } = string.Empty;
//         [Required] public string LastName { get; set; } = string.Empty;
//         [Required] public DateOnly DateOfBirth { get; set; }
//         [Required] public string Gender { get; set; } = string.Empty;
//         [Required] public string BvnPhone { get; set; } = string.Empty;
//     }

//     public class Handler : IRequestHandler<Command, BaseResult>
//     {
//         public async Task<BaseResult> Handle(Command request, CancellationToken ct)
//         {
//             var publicId = _currentUser.PublicId; // Get logged-in user
            
//             var bvnHash = _hasher.Hash(request.Bvn, "BVN");
//             var bvnEncrypted = _encryptionService.Encrypt(request.Bvn);

//             // Update the User entity
//             var user = await _context.Users.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
            
//             user.FirstName = request.FirstName;
//             user.LastName = request.LastName;
//             user.DateOfBirth = request.DateOfBirth;
//             user.Gender = Enum.Parse<Gender>(request.Gender); // Map string to enum
//             user.BvnPhone = request.BvnPhone;
//             user.BvnHash = bvnHash;
//             user.BvnEncrypted = bvnEncrypted;

//             await _context.SaveChangesAsync(ct);

//             return new BaseResult(HttpStatusCode.OK, "Details confirmed.");
//         }
//     }
// }