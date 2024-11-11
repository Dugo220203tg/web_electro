using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;
namespace API_Web_Shop_Electronic_TD.Interfaces

{
	public interface IDanhMuc
	{
		Task<List<DanhMucSp>> GetAllAsync();
		Task<DanhMucSp?> GetByIdAsync(int MaDanhMuc);
		//Task<DanhMuc?> GetByNameAsync(string TenLoai);
		Task<DanhMucSp> CreateAsync(DanhMucMD model);
		Task<DanhMucSp?> UpdateAsync(int MaDanhMuc, DanhMucMD model);
		Task<DanhMucSp?> DeleteAsync(int MaDanhMuc);
	}
}
