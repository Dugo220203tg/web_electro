using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface IHangHoaRepository
	{
		Task<List<HangHoa>> GetAllAsync();
		Task<HangHoa?> GetByIdAsync(int Mahh);
		Task<HangHoa> CreateAsync(CreateHangHoaMD model);
		Task<HangHoa?> UpdateAsync(int Mahh, UpdateHangHoaMD model);
		Task<HangHoa?> DeleteAsync(int Mahh);
	}
	public interface IDanhGiaSp
	{
		Task<List<DanhGiaSp>> GetAllAsync();
		Task<DanhGiaSp?> GetByIdAsync(int MaDG);
		Task<DanhGiaSp> CreateAsync(CreateDanhGiaSpMD model);
		Task<DanhGiaSp?> UpdateAsync(int MaDG, CreateDanhGiaSpMD model);
		Task<DanhGiaSp?> DeleteAsync(int MaDG);
	}	

}
