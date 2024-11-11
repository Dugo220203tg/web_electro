using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class TrangThaiHdRepository : ITrangThaiHd
	{
		private readonly Hshop2023Context db;
		public TrangThaiHdRepository(Hshop2023Context db)
		{
			this.db = db;
		}
		public async Task<List<TrangThai>> GetAllAsync()
		{
			return await db.TrangThais.ToListAsync();

		}
	}
}
