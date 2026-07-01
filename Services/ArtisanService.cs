using LocalArtisanCraftMarket.Models;
using LocalArtisanCraftMarket.Repositories;

namespace LocalArtisanCraftMarket.Services
{
    internal class ArtisanService
    {
        private readonly ArtisanRepository _repo = new ArtisanRepository();

        public (bool Success,string Message,int Id) Register(Artisan artisan)
        {
            if (string.IsNullOrWhiteSpace(artisan.FullName)) return (false,"Name required",-1);
            if (string.IsNullOrWhiteSpace(artisan.Email)) return (false,"Email required",-1);
            if (_repo.EmailExists(artisan.Email)) return (false,"Email already used",-1);
            int id = _repo.Create(artisan);
            return (true,"Registered",id);
        }
    }
}
