﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;

namespace TDProjectMVC.ViewModels
{
    public class DanhGiaVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDG { get; set; }
        public string MaKH { get; set; }
        public int Sao { get; set; }
        public int MaHH { get; set; }
        public string NoiDung { get; set; }
        public DateTime Ngay { get; set; }
        public int TrungBinhSao { get; set; }
        public double MotSao { get; set; }
        public double HaiSao { get; set; }
        public double BaSao { get; set; }
        public double BonSao { get; set; }
        public double NamSao { get; set; }
        public int TrangThai { get; set; }
        public double TotalRatings => NamSao + BonSao + BaSao + HaiSao + MotSao;

        public double NamSaoPercentage => TotalRatings > 0 ? (double)NamSao / TotalRatings * 100 : 0;
        public double BonSaoPercentage => TotalRatings > 0 ? (double)BonSao / TotalRatings * 100 : 0;
        public double BaSaoPercentage => TotalRatings > 0 ? (double)BaSao / TotalRatings * 100 : 0;
        public double HaiSaoPercentage => TotalRatings > 0 ? (double)HaiSao / TotalRatings * 100 : 0;
        public double MotSaoPercentage => TotalRatings > 0 ? (double)MotSao / TotalRatings * 100 : 0;
    }

    public class DanhGiaListViewModel
    {
        public int ReviewCount { get; set; }
        public List<DanhGiaVM> DanhGias { get; set; } = new List<DanhGiaVM>(); // Initialize with empty list
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int MaHH { get; set; }
    }
}
