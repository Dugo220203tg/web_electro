document.addEventListener('DOMContentLoaded', function () {
    // Utility functions
    const showAlert = (icon, title, text, timer = 1500) => {
        Swal.fire({ icon, title, text, showConfirmButton: false, timer });
    };

    const ajaxRequest = (url, method, data, successCallback, errorCallback) => {
        $.ajax({
            url, method, data,
            success: (result) => {
                if (result.success) {
                    successCallback(result);
                } else {
                    showAlert("error", "Lỗi", result.message);
                }
            },
            error: (xhr, status, error) => {
                console.error(`Error: ${error}, Response: ${xhr.responseText}`);
                errorCallback(xhr, error);
            }
        });
    };

    const getImageHtml = (item) => {
        const firstImageUrl = item.hinh ? item.hinh.split(',')[0].trim() : '';
        return firstImageUrl ? `<img src="/Hinh/Hinh/HangHoa/${item.maHH}/${firstImageUrl}" alt="${item.tenHH}" style="width:60px;height:60px">` : '';
    };

    // Cart functions
    window.addToCart = (id, quantity, type) => {
        ajaxRequest(
            "/Cart/AddToCart",
            "POST",
            { id, quantity, type },
            (result) => {
                showAlert("success", "Thành công", "Sản phẩm đã được thêm vào giỏ hàng");
                updateCartDisplay();
            },
            (xhr, error) => showAlert("error", "Lỗi", error)
        );
    };

    const updateCartDisplay = () => {
        ajaxRequest(
            "/Cart/GetCartData",
            "GET",
            null,
            (data) => {
                $(".qty").text(data.totalQuantity);
                $(".cart-list").empty();

                data.cardProducts.forEach(item => {
                    const productHtml = `
                        <div class="product-widget">
                            <div class="product-img" style="width:60px;height:60px">${getImageHtml(item)}</div>
                            <div class="product-body">
                                <h2 class="product-name" style="display:-webkit-box;-webkit-box-orient:vertical;-webkit-line-clamp:2;overflow:hidden;text-overflow:ellipsis;">
                                    <a href="/Product/Detail/${item.maHH}">${item.tenHH}</a>
                                </h2>
                                <h4 class="product-price"><span class="qty">${item.soLuong} x</span>$${item.donGia}</h4>
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
            (xhr, error) => console.error(error)
        );
    };

    // Wishlist functions
    window.addToWishlist = (id) => {
        ajaxRequest(
            "/WishList/AddToWishList",
            "POST",
            { MaHH: id },
            (result) => {
                showAlert("success", "Thành công", result.message);
                updateWishList();
            },
            (xhr, error) => showAlert("error", "Lỗi", "Đã xảy ra lỗi khi thêm sản phẩm vào danh sách yêu thích")
        );
    };

    window.RemoveWishList = (id, element) => {
        ajaxRequest(
            "/WishList/RemoveWishList",
            "POST",
            { id },
            (result) => {
                showAlert("success", "Xóa thành công", "Đã xóa sản phẩm khỏi danh sách yêu thích", 1000);
                $(element).closest(".product-widget").remove();
                updateWishList();
            },
            (xhr, error) => {
                const errorMessage = xhr.status === 409 ? "Sản phẩm đã bị xóa hoặc thay đổi trước đó." : "Đã xảy ra lỗi khi xóa sản phẩm khỏi danh sách yêu thích";
                showAlert("error", "Lỗi", `${errorMessage}: ${xhr.responseText}`);
            }
        );
    };

    const updateWishList = () => {
        ajaxRequest(
            "/WishList/Index",
            "GET",
            null,
            (data) => {
                $('.wish-list').empty();
                data.forEach(item => {
                    const productHtml = `
                        <div class="product-widget">
                            <div class="product-img" style="width:60px;height:60px">${getImageHtml(item)}</div>
                            <div class="product-body">
                                <h2 class="product-name" style="display:-webkit-box;-webkit-box-orient:vertical;-webkit-line-clamp:2;overflow:hidden;text-overflow:ellipsis;">
                                    <a href="#">${item.tenHH}</a>
                                </h2>
                            </div>
                            <button class="delete" onclick="RemoveWishList(${item.maYT}, this)">
                                <i class="fa fa-close"></i>
                            </button>
                        </div>
                    `;
                    $('.wish-list').append(productHtml);
                });
            },
            (xhr, error) => console.error(error)
        );
    };

    // Event listeners and initializations
    $(".add-to-cart-btn").on("click", function () {
        const id = $(this).data("id");
        addToCart(id, 1, "Normal");
    });

    $("#searchButton").click(function () {
        const searchText = $("#searchText").val().trim();
        window.location.href = searchText ? `/Product/Search?query=${encodeURIComponent(searchText)}` : "/Product";
    });

    $(".section-tab-nav li").click(function () {
        $(".section-tab-nav li").removeClass("active");
        $(this).addClass("active");
    });

    const queryParams = new URLSearchParams(window.location.search);
    const loaiParam = queryParams.get("loai");
    if (loaiParam) {
        $(`.section-tab-nav li a[asp-route-loai="${loaiParam}"]`).parent().addClass("active");
    }

    $(".page-link").click(function (e) {
        e.preventDefault();
        console.log("Chuyển đến trang: " + $(this).text());
    });

    const urlPath = window.location.pathname + window.location.search;
    $(".danhmuc").each(function () {
        const href = $(this).attr("href");
        if (href && urlPath.includes(href)) {
            const labelContent = $(this).find("label").text().trim();
            $("#breadcrumb-tree li.active").text(labelContent);
        }
    });

    updateWishList();
    updateCartDisplay();
});