
//---------------------------------------------Your Cart-----------------------------------------------
document.addEventListener('DOMContentLoaded', function () {
    // Định nghĩa hàm addToCart trong phạm vi toàn cục
    window.addToCart = function (id, quantity, type) {
        console.log("ID sản phẩm:", id); // Kiểm tra ID sản phẩm được truyền vào
        if (!id || id <= 0) {
            Swal.fire({
                icon: "error",
                title: "Lỗi",
                text: "Mã sản phẩm không hợp lệ.",
            });
            return;
        }
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
                    updateCartDisplay(); // Cập nhật hiển thị giỏ hàng
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
    };


    // Định nghĩa hàm updateCartDisplay
    function updateCartDisplay() {
        $.ajax({
            url: "/Cart/GetCartData",
            type: "GET",
            success: function (data) {
                $(".qty").text(data.totalQuantity);
                $(".cart-list").empty();

                $.each(data.cardProducts, function (index, item) {
                    var firstImageUrl = '';
                    if (item.hinh) {
                        var imageUrls = item.hinh.split(',');
                        if (imageUrls.length > 0) {
                            firstImageUrl = imageUrls[0].trim();
                        }
                    }

                    var imgHtml = firstImageUrl ?
                        `<img src="/Hinh/Hinh/HangHoa/${item.maHH}/${firstImageUrl}" alt="${item.tenHH}" style="width:60px;height:60px">` :
                        '';

                    var productHtml = `
                            <div class="product-widget">
                                <div class="product-img" style="width:60px;height:60px">
                                    ${imgHtml}
                                </div>
                                <div class="product-body">
                                    <h2 class="product-name" style="display: -webkit-box; -webkit-box-orient: vertical; -webkit-line-clamp: 2; overflow: hidden; text-overflow: ellipsis;">
                                        <a href="/Product/Detail/${item.maHH}">${item.tenHH}</a>
                                    </h2>
                                    <h4 class="product-price"><span class="qty">${item.soLuong} x</span>$ ${item.donGia}</h4>
                                </div>
                                <button class="delete" onclick="RemoveCart(${item.maHH}, this)">
                                    <i class="fa fa-times text-danger"></i>
                                </button>
                            </div>
                            `;
                    $(".cart-list").append(productHtml);
                });

                $(".cart-summary h5").text(`SUBTOTAL: $${data.totalAmount}`);
                $(".cart-summary small").text(`${data.totalQuantity} Item(s) selected`);
            },
            error: function (xhr, status, error) {
                console.log(error);
            },
        });
    }
    function RemoveCart(productId, buttonElement) {
        $.ajax({
            url: '/Cart/RemoveCart', // Replace with the actual controller name
            type: 'POST',
            data: { id: productId },
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: "success",
                        title: "Xóa thành công",
                        text: "Đã xóa sản phẩm khỏi danh",
                        showConfirmButton: false,
                        timer:1000,
                    }).the(() => {
                        $(buttonElement).closest('tr').remove();
                        $(element).closest(".product-widget").remove();
                        updateCartDisplay();
                    })
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Lỗi",
                        text: result.message,
                    });
                }
            },
            error: function (xhr) {
                let errorMessage = "Đã xảy ra lỗi khi xóa sản phẩm khỏi danh sách";
                if (xhr.status === 409) {
                    errorMessage = "Sản phẩm đã bị xóa hoặc thay đổi trước đó.";
                }
                Swal.fire({
                    icon: "error",
                    title: "Lỗi",
                    text: errorMessage,
                });
            },
        });
    }
    // Gán sự kiện click cho các nút "Thêm vào giỏ hàng"
    $(".add-to-cart-btn").on("click", function () {
        var id = $(this).data("id");
        var quantity = 1; // hoặc lấy từ input số lượng nếu có
        addToCart(id, quantity, "Normal");
    });
});

//---------------------------------------------Your Cart-----------------------------------------------
// Tìm kiếm
$(document).ready(function () {
    $("#searchButton").click(function () {
        let searchText = $("#searchText").val().trim(); // Trim to remove leading/trailing spaces
        if (searchText !== "") {
            // Check if search text is not empty
            let url = `/Product/Search?query=` + encodeURIComponent(searchText); // Encode search text
            window.location.href = url;
        } else {
            window.location.href = "/Product"; // Redirect to /Product if searchText is empty
        }
    });
});



// Thêm sự kiện click cho các thẻ li
var liElements = document.querySelectorAll(".section-tab-nav li");
liElements.forEach(function (li) {
    li.addEventListener("click", function () {
        // Xóa lớp "active" từ tất cả các thẻ li
        liElements.forEach(function (el) {
            el.classList.remove("active");
        });
        // Thêm lớp "active" cho thẻ li được nhấp vào
        this.classList.add("active");
    });
});

// Kiểm tra nếu có tham số loại trong URL, thêm lớp "active" cho thẻ li tương ứng
var queryParams = new URLSearchParams(window.location.search);
var loaiParam = queryParams.get("loai");
if (loaiParam) {
    var activeLi = document.querySelector(
        '.section-tab-nav li a[asp-route-loai="' + loaiParam + '"]'
    ).parentNode;
    if (activeLi) {
        activeLi.classList.add("active");
    }
}
// Pagination link click handler
$(".page-link").click(function (e) {
    e.preventDefault();
    var page = $(this).text();
    // console.log("Chuyển đến trang: " + page);
});

// Hiển thị danh mục sản phẩm trên breadcrumb
document.addEventListener("DOMContentLoaded", function () {
    // Lấy phần path của URL
    var urlPath = window.location.pathname + window.location.search;
    //console.log("urlPath:", urlPath);
    // Lặp qua tất cả các phần tử có class "danhmuc"
    document.querySelectorAll(".danhmuc").forEach((link) => {
        // Lấy phần tử label trong phạm vi của thẻ <a>
        var label = link.querySelector("label");
        // Kiểm tra xem label có tồn tại không trước khi lấy dữ liệu từ nó
        if (label) {
            // Lấy nội dung của label
            var labelContent = label.textContent.trim();
            // Cập nhật nội dung của phần tử <li> có class 'active' trong breadcrumb
            document.querySelector("#breadcrumb-tree li.active").textContent =
                labelContent;
        }
    });
});
//---------------------------------------------Your Cart-----------------------------------------------



//---------------------------------------------WISH-LIST-----------------------------------------------
// Function to add item to wishlist
function addToWishlist(id) {
    $.ajax({
        url: "/WishList/AddToWishList",
        type: "POST",
        data: { MaHH: id },
        success: function (result) {
            if (result.success) {
                Swal.fire({
                    icon: "success",
                    title: "Thành công",
                    text: result.message,
                    showConfirmButton: false,
                    timer: 1500,
                });
                updateWishList(); // Update the wishlist display
                updateWishListCount(); // Update the wishlist count
            } else {
                Swal.fire({
                    icon: "error",
                    title: "Lỗi",
                    text: result.message,
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Lỗi",
                text: "Đã xảy ra lỗi khi thêm sản phẩm vào danh sách yêu thích",
            });
        },
    });
}

// Function to remove an item from the wishlist
function RemoveWishList(id, element) {
    $.ajax({
        url: "/WishList/RemoveWishList",
        type: "POST",
        data: { id: id },
        success: function (result) {
            if (result.success) {
                Swal.fire({
                    icon: "success",
                    title: "Xóa thành công",
                    text: "Đã xóa sản phẩm khỏi danh sách yêu thích",
                    showConfirmButton: false,
                    timer: 1000,
                }).then(() => {
                    $(element).closest(".product-widget").remove();
                    updateWishList(); // Update the wishlist display
                    updateWishListCount(); // Update the wishlist count
                });
            } else {
                Swal.fire({
                    icon: "error",
                    title: "Lỗi",
                    text: result.message,
                });
            }
        },
        error: function (xhr) {
            let errorMessage = "Đã xảy ra lỗi khi xóa sản phẩm khỏi danh sách yêu thích";
            if (xhr.status === 409) {
                errorMessage = "Sản phẩm đã bị xóa hoặc thay đổi trước đó.";
            }
            Swal.fire({
                icon: "error",
                title: "Lỗi",
                text: errorMessage,
            });
        },
    });
}

// Function to update the wishlist count
function updateWishListCount() {
    $.ajax({
        url: "/WishList/GetWishListCount",
        type: "GET",
        success: function (data) {
            $('.qty_WishList').text(data.count); // Update count in the UI
        },
        error: function () {
            console.log("Error updating wishlist count");
        }
    });
}
function updateWishList() {
    $.ajax({
        url: "/WishList/updateWishList",
        type: "GET",
        success: function (data) {
            $('.wish-list').empty(); // Làm trống danh sách trước khi cập nhật
            $.each(data, function (index, item) {
                var productHtml = `
             <div class="product-widget">
                    <div class="product-img" style="width:60px;height:60px">
                        ${(() => {
                                let firstImageUrl = '';
                                if (item.hinh) {
                                    const imageUrls = item.hinh.split(',');
                                    if (imageUrls.length > 0) {
                                        firstImageUrl = imageUrls[0].trim();
                                    }
                                }
                                return firstImageUrl
                                    ? `<img src="/Hinh/Hinh/HangHoa/${item.maHH}/${firstImageUrl}" alt="${item.tenHH}" style="width:60px;height:60px">`
                                    : '';
                            })()}
                    </div>
                    <div class="product-body" style="position: relative;">
                        <h2 class="product-name" style="display: -webkit-box; -webkit-box-orient: vertical; -webkit-line-clamp: 2; overflow: hidden; text-overflow: ellipsis;">
                            <a href="#">${item.tenHH}</a>
                        </h2>
                        <div class="product-info" style="display: flex; align-items: center;">
                            <h4 class="product-price">${parseInt(item.donGia).toLocaleString('vi-VN')} VNĐ</h4>
                            <i class="fa fa-shopping-cart icon-cart"
                               style="font-size:24px; color:#D10024; pointer-events:auto;"
                               onclick="addToCart(${item.maHH}, 1, 'Normal')"></i>
                        </div>
                    </div>
                    <button class="delete" data-product-id="${item.maYT}" onclick="RemoveWishList(${item.maYT}, this)">
                        <i class="fa fa-close"></i>
                    </button>
                </div>
            `;
                $('.wish-list').append(productHtml);
            });
        },
        error: function (xhr, status, error) {
            console.log(error); // Log lỗi nếu không thể lấy danh sách yêu thích
        }
    });
}

// Initialize wishlist on document ready
$(document).ready(function () {
    updateWishList(); // Cập nhật danh sách yêu thích khi trang được tải
});
//---------------------------------------------WISH-LIST-----------------------------------------------

