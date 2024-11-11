$(document).ready(function () {
    // Toggle dropdown on click
    $(".dropdown-toggle").on("click", function (e) {
        e.preventDefault();
        $(this).parent().toggleClass("open");
        $(this).attr("aria-expanded", function (index, attr) {
            return attr === "true" ? "false" : "true";
        });
    });

    // Close dropdown when clicking outside
    $(document).on("click", function (e) {
        if (!$(e.target).closest(".dropdown").length) {
            $(".dropdown").removeClass("open");
            $(".dropdown-toggle").attr("aria-expanded", "false");
        }
    });

    // Cập nhật giỏ hàng sau khi thêm sản phẩm
    function updateCartDisplay() {
        $.ajax({
            url: "/Cart/GetCartData",
            type: "GET",
            success: function (data) {
                // Cập nhật số lượng sản phẩm trong giỏ hàng
                $(".qty").text(data.totalQuantity);

                // Xóa danh sách sản phẩm hiện tại
                $(".cart-list").empty();

                // Thêm danh sách sản phẩm mới
                $.each(data.cardProducts, function (index, item) {
                    // Lấy URL ảnh sản phẩm (nếu có)
                    let firstImageUrl = "";
                    if (item.Hinh) {
                        const imageUrls = item.Hinh.split(",");
                        if (imageUrls.length > 0) {
                            firstImageUrl = imageUrls[0].trim();
                        }
                    }
                    // Chèn timestamp để tránh cache hình ảnh cũ
                    const imageUrl = firstImageUrl
                        ? `/Hinh/Hinh/HangHoa/${item.MaHH}/${firstImageUrl}?t=${new Date().getTime()}`
                        : "";

                    // HTML cho sản phẩm trong giỏ
                    var productHtml = `
                    <div class="product-widget">
                        <div class="product-img" style="width:60px;height:60px">
                            ${imageUrl
                            ? `<img src="${imageUrl}" alt="${item.TenHH}" style="width:60px;height:60px">`
                            : ""
                        }
                        </div>
                        <div class="product-body">
                            <h2 class="product-name"><a href="#">${item.tenHH
                        }</a></h2>
                            <h4 class="product-price"><span class="qty">${item.soLuong
                        } x</span>$${item.donGia}</h4>
                        </div>
                        <button class="delete" data-product-id="${item.maHH}">
                            <i class="fa fa-times text-danger"></i>
                        </button>
                    </div>
                  `;
                    $(".cart-list").append(productHtml);
                });

                // Gán sự kiện click cho các nút "Xóa" mới
                $(".cart-list .delete").on("click", function () {
                    var productId = $(this).data("product-id");
                    var productWidget = $(this).closest(".product-widget");
                    RemoveCart(productId, productWidget);
                });

                // Cập nhật tổng tiền
                $(".cart-summary h5").text(`SUBTOTAL: $${data.totalAmount}`);
            },
            error: function (xhr, status, error) {
                console.log(error);
            },
        });
    }

    // Thêm sản phẩm vào giỏ hàng
    function addToCart(id, quantity, type) {
        $.ajax({
            url: "/Cart/AddToCart",
            type: "POST",
            data: { id: id, quantity: quantity, type: type },
            success: function (result) {
                if (result.success) {
                    Swal.fire({
                        icon: "success",
                        title: "Thành công",
                        text: "Sản phẩm đã được thêm vào giỏ hàng",
                        showConfirmButton: false,
                        timer: 1500,
                    });
                    updateCartDisplay(); // Cập nhật hiển thị giỏ hàng ngay lập tức
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Lỗi",
                        text: result.message,
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: "error",
                    title: "Lỗi",
                    text: error,
                });
            },
        });
    }

    // Xóa sản phẩm khỏi giỏ hàng
    function RemoveCart(id, element) {
        $.ajax({
            url: "/Cart/RemoveCart",
            type: "POST",
            data: { id: id },
            success: function (result) {
                if (result.success) {
                    Swal.fire({
                        icon: "success",
                        title: "Xóa Thành công",
                        text: "Đã xóa sản phẩm khỏi giỏ hàng",
                        showConfirmButton: false,
                        timer: 1000,
                    });
                    updateCartDisplay(); // Cập nhật hiển thị giỏ hàng
                    $(element).closest("tr").remove(); // Xóa phần tử HTML
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Lỗi",
                        text: result.message,
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: "error",
                    title: "Lỗi",
                    text: error,
                });
            },
        });
    }

    // Hiển thị Your Cart
    $("a.dropdown-toggle").click(function (e) {
        e.preventDefault();
        $(this).attr("aria-expanded", function (index, attr) {
            return attr === "true" ? "false" : "true";
        });
        $(this).closest("div.dropdown").toggleClass("open");
    });
});