using dvcsharp_core_api.Data;
using dvcsharp_core_api.Models;

namespace dvcsharp_core_api.Service;

public interface IUserService
{
    void UpdatePassword(ref User user, string password);
    string CreateAccessToken(User user);

    AuthorizationResponse AuthorizeCreateAccessToken(
        GenericDataContext _context,
        AuthorizationRequest authorizationRequest);
    
    


}