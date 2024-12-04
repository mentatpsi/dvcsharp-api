using dvcsharp_core_api.Data;
using dvcsharp_core_api.Models;

namespace dvcsharp_core_api.Service;

public interface IUserService
{
    void updatePassword(ref User user, string password);
    string createAccessToken(User user);

    AuthorizationResponse authorizeCreateAccessToken(
        GenericDataContext _context,
        AuthorizationRequest authorizationRequest);
    
    


}