﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TrangQuanLy.Models
{
    public class HangHoaVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHH { get; set; }
        [Required]
        public string TenHH { get; set; }
        [Required]
        public string Hinh { get; set; }
        [Required]
        public string MoTa { get; set; }
        public string MoTaDonVi { get; set; }
        [Required]
        public int MaLoai { get; set; }
        public DateOnly NgaySX { get; set; }
        public double GiamGia { get; set; }
        [Required]
        public string MaNCC { get; set; }
        public double DonGia { get; set; }
        public double SoLanXem { get; set; }
        public int SoLuong { get; set; }
        public string TenLoai {  get; set; }
        public string TrangThai
        {
            get
            {
                return SoLuong == 0 ? "Hết hàng" : "Còn hàng";
            }
        }
        public string TenDanhMuc { get; set; }
        public int maDanhMuc { get; set; }
    }
    public class AllHangHoaVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHH { get; set; }
        public string TenHH { get; set; }
        public string Hinh { get; set; }
        public string MoTa { get; set; }
        public string MoTaDonVi { get; set; }
        public string MaLoai { get; set; }
        public DateOnly NgaySX { get; set; }
        public double GiamGia { get; set; }
        public string MaNCC { get; set; }
        public double DonGia { get; set; }
        public double SoLanXem { get; set; }
    }
    public class CreateHangHoaVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHH { get; set; }
        public string TenHH { get; set; }
        public string Hinh { get; set; }
        public string MoTa { get; set; }
        public string MoTaDonVi { get; set; }
        public DateOnly NgaySX { get; set; }
        public double GiamGia { get; set; }
        public double DonGia { get; set; }
        public int MaLoai { get; set; }
        public string MaNCC { get; set; }


    }
    public class EditHangHoaVM
    {
        public int MaHH { get; set; }
        public string TenHH { get; set; }
        public string Hinh { get; set; }
        public string MoTa { get; set; }
        public string MoTaDonVi { get; set; }
        public DateOnly NgaySX { get; set; }
        public double GiamGia { get; set; }
        public double DonGia { get; set; }
        public int MaLoai { get; set; }
        public string MaNCC { get; set; }
    }
}
