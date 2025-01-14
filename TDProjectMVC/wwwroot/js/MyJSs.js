
// Document Ready Handler
$(document).ready(function () {
    // Handle success/error messages
    handleMessages();

    // Initialize reviews
    //initializeReviews();

    // Initialize comment submission
    initializeCommentSubmission();

    // Initialize order view handling
    //initializeOrderView();

    // Initialize checkout form
    //initializeCheckout();

    // Initialize address autocomplete
    initAddressAutocomplete();
});

// Message Handler
function handleMessages() {
    // Get message elements that were rendered by Razor
    const successMessage = document.getElementById('temp-success-message');
    const errorMessage = document.getElementById('temp-error-message');

    // Check if elements exist and have content
    if (successMessage && successMessage.value.trim()) {
        Swal.fire({
            icon: 'success',
            title: 'Thông báo',
            text: successMessage.value
        });
    } else if (errorMessage && errorMessage.value.trim()) {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: errorMessage.value
        });
    }
}
// Brand and Category Toggle Functions
function toggleBrands() {
    var moreBrands = document.getElementById('more-brands');
    var toggleButton = document.getElementById('toggle-brands');

    if (moreBrands.style.display === 'none') {
        moreBrands.style.display = 'block';
        toggleButton.textContent = 'Show Less';
    } else {
        moreBrands.style.display = 'none';
        toggleButton.textContent = 'Show More';
    }
}

function toggleLoais() {
    var toggleloaiButton = document.getElementById('toggle-loais');
    var moreLoais = document.getElementById('more-loais');

    if (moreLoais.style.display === 'none') {
        moreLoais.style.display = 'block';
        toggleloaiButton.textContent = 'Show Less';
    } else {
        moreLoais.style.display = 'none';
        toggleloaiButton.textContent = 'Show More';
    }
}

// Price Filter Initialization
function initializePriceFilter() {
    document.addEventListener("DOMContentLoaded", function () {
        // Initialize price slider
        const priceSlider = document.getElementById("price-slider");
        const filterPriceBtn = document.getElementById("filterPriceBtn");

        if (priceSlider && filterPriceBtn) {
            noUiSlider.create(priceSlider, {
                start: [0, 100000000],
                connect: true,
                range: {
                    min: 0,
                    max: 100000000,
                },
                step: 10000,
                format: {
                    to: function (value) {
                        return Math.round(value);
                    },
                    from: function (value) {
                        return Number(value);
                    },
                },
            });
            // Update price labels dynamically
            const minPriceElem = document.getElementById("min-price");
            const maxPriceElem = document.getElementById("max-price");
            if (minPriceElem && maxPriceElem) {
                priceSlider.noUiSlider.on("update", function (values) {
                    minPriceElem.textContent = `VND ${parseInt(values[0]).toLocaleString()}`;
                    maxPriceElem.textContent = `VND ${parseInt(values[1]).toLocaleString()}`;
                });
            }

            // Handle price filtering
            filterPriceBtn.addEventListener("click", function () {
                const [minPrice, maxPrice] = priceSlider.noUiSlider.get();
                const params = new URLSearchParams(window.location.search);
                params.set("minPrice", minPrice);
                params.set("maxPrice", maxPrice);
                const newUrl = `${window.location.pathname}?${params.toString()}`;
                window.location.href = newUrl;
            });

            // Handle sorting
            const sortProduct = document.querySelector(".sort-product");
            if (sortProduct) {
                sortProduct.addEventListener("change", function (event) {
                    const selectedOption = event.target.value;
                    const params = new URLSearchParams(window.location.search);

                    params.delete("sortOrder");

                    switch (selectedOption) {
                        case "0":
                            params.set("sortOrder", "price_desc");
                            break;
                        case "1":
                            params.set("sortOrder", "price_asc");
                            break;
                        case "2":
                            params.set("sortOrder", "popularity");
                            break;
                        default:
                            params.delete("sortOrder");
                    }

                    const newUrl = `${window.location.pathname}?${params.toString()}`;
                    window.location.href = newUrl;
                });
            }
        }
    });
}
// Order Cancellation
function confirmCancelOrder(orderId, btn) {
    Swal.fire({
        title: 'Bạn có chắc chắn muốn hủy đơn hàng?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Đồng ý',
        cancelButtonText: 'Từ chối',
    }).then((result) => {
        if (result.isConfirmed) {
            // Gọi Ajax để hủy đơn hàng khi người dùng chọn "Đồng ý"
            $.ajax({
                url: '/DonHang/HuyDonHang',  // Đường dẫn đến Action HuyDonHang
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ MaHD: orderId }),  // Gửi mã đơn hàng qua model
                success: function (response) {
                    if (response.success) {  // Kiểm tra đúng thuộc tính success từ phản hồi
                        Swal.fire({
                            icon: 'success',
                            title: 'Thành công',
                            text: response.message,
                            showConfirmButton: false,
                            timer: 2000
                        }).then(() => {
                            location.reload();  // Reload lại trang sau khi thành công
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Lỗi',
                            text: response.message || 'Đã xảy ra lỗi khi xử lý!',
                        });
                    }
                },
                error: function (xhr, status, error) {
                    let errorMessage = xhr.responseJSON && xhr.responseJSON.message
                        ? xhr.responseJSON.message
                        : 'Đã xảy ra lỗi khi hủy đơn hàng. Vui lòng thử lại!';
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: errorMessage,
                    });
                }
            });
        }
    });
}

// Reviews Functions
function loadReviewPage(maHH, page) {
    function loadReviewPage(maHH, page) {
        if (!maHH || !page) return;
        $('#reviews-content').fadeOut(200, function () {
            $.ajax({
                url: `/Product/LoadReviews`,
                type: 'GET',
                data: { maHH: maHH, page: page },
                success: function (result) {
                    $('#reviews').html(result);
                    $('#reviews-content').fadeIn(200);
                },
                error: function (err) {
                    console.error('Error loading reviews:', err);
                    toastr.error('Có lỗi xảy ra khi tải đánh giá');
                    $('#reviews-content').fadeIn(200);
                }
            });
        });
    }
    $(document).ready(function () {
        // Any initialization code here
    });
}
$(document).ready(function () {
    // Handle View button click
    $(".view-order").click(function () {
        const orderId = $(this).data("id"); // Get the order ID

        // Hide all orders and remove 'active' class
        $(".order-details-nb").removeClass("active").addClass("d-none");

        // Show the clicked order
        $(`.order-details-nb[data-id="${orderId}"]`).removeClass("d-none").addClass("active");

        // Highlight the selected row in the order list
        $("tr").removeClass("selected"); // Remove highlight from all rows
        $(this).closest("tr").addClass("selected"); // Add highlight to the clicked row
    });
});
$(document).ready(function () {
    $("#GiongKhachHang").change(function () {
        if ($(this).prop("checked")) {
            $(this).val(true);
            $(".delivery-info").addClass("d-none");
        } else {
            $(this).val(false);
            $(".delivery-info").removeClass("d-none");
        }
    });
});
// Comment Submission
function initializeCommentSubmission() {
    $(document).ready(function () {
        $('#submitComment').click(function () {
            var MaHH = $('#MaHH').val();
            var noiDung = $('#noiDung').val();
            var sao = $('input[name="rating"]:checked').val();

            if (!sao) {
                Swal.fire({
                    icon: "warning",
                    title: "Rating Missing",
                    text: "Please select a rating.",
                });
                return;
            }

            $.ajax({
                url: '/KhachHang/AddComment',
                type: 'POST',
                data: {
                    MaHH: MaHH,
                    noiDung: noiDung,
                    sao: sao
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: "success",
                            title: "Success",
                            text: "Comment submitted successfully!",
                            showConfirmButton: false,
                            timer: 1500
                        });
                        $('#noiDung').val(''); // Clear the comment field
                        $('input[name="rating"]').prop('checked', false); // Reset rating selection
                    } else {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: response.message,
                        });
                    }
                },
                error: function (xhr, status, error) {
                    if (xhr.status === 401) {
                        Swal.fire({
                            icon: "error",
                            title: "Unauthorized",
                            text: "Please log in to submit a comment.",
                        });
                        // Optionally, redirect to login page
                        window.location.href = '/KhachHang/DangNhap';
                    } else {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: 'Error submitting comment.',
                        });
                    }
                }
            });
        });
    });
}

// Cart Functions
function RemoveCart(productId, buttonElement) {
    $.ajax({
        url: '/Cart/RemoveCart',
        type: 'POST',
        data: { id: productId },
        success: function (response) {
            if (response.success) {
                Swal.fire({
                    icon: "success",
                    title: "Xóa thành công",
                    text: "Đã xóa sản phẩm khỏi danh sách",
                    showConfirmButton: false,
                    timer: 1000,
                }).then(() => {
                    $(buttonElement).closest(".product-widget").remove();
                    updateCartDisplay();
                });
            } else {
                Swal.fire({
                    icon: "error",
                    title: "Lỗi",
                    text: response.message,
                });
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            Swal.fire({
                icon: "error",
                title: "Lỗi",
                text: "Đã có lỗi xảy ra khi xóa sản phẩm khỏi giỏ hàng",
            });
        }
    });
}

// Category Functions
function initializeCategories() {
    function getQueryParam(param) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(param);
    }

    // Set checkbox state based on the 'danhmuc' query parameter in the URL
    const selecteddanhmuc = getQueryParam('danhmuc');
    if (selecteddanhmuc) {
        document.querySelectorAll('.danhmuc-checkbox').forEach(function (checkbox) {
            if (checkbox.id === selecteddanhmuc) {
                checkbox.checked = true;
            }
        });
    }

    // Event listener for checkboxes
    document.querySelectorAll('.danhmuc-checkbox').forEach(function (checkbox) {
        checkbox.addEventListener('change', function () {
            // Uncheck all other checkboxes if this one is checked
            if (checkbox.checked) {
                // Redirect to the new URL based on the selected checkbox
                window.location.href = checkbox.dataset.url;
            } else {
                // If checkbox is unchecked, go back to the original page (remove 'danhmuc' from the URL)
                const currentUrl = new URL(window.location.href);
                currentUrl.searchParams.delete('danhmuc'); // Remove 'danhmuc' param
                window.location.href = currentUrl.href; // Reload the page without 'danhmuc'
            }
        });
    });
}

// Address Autocomplete
function initAddressAutocomplete() {
    const addressInput = document.getElementById('address-input');
    const suggestionsList = document.getElementById('suggestions-list');
    let shippingValue = $('.order-col:has(div:contains("Shipping")) div strong').text();
    const totalElement = document.querySelector('.order-total');
    let currentShippingFee = 0;

    if (!addressInput || !suggestionsList) return;

    let debounceTimer;

    // Format currency function
    function formatCurrency(amount) {
        // Convert to number and handle invalid cases
        const numericAmount = Number(amount);
        if (isNaN(numericAmount)) {
            return "0 VNĐ";
        }
        // Format với dấu phẩy ngăn cách hàng nghìn
        return numericAmount.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") + " VNĐ";
    }
    // Calculate new total
    function updateTotal(shippingFee) {
        // Convert shippingFee to number if it's string
        shippingFee = Number(shippingFee) || 0;

        // Get subtotal
        const subtotalElement = document.querySelector('.order-products');
        const subtotal = subtotalElement ? parseFloat(subtotalElement.getAttribute('data-subtotal')) || 0 : 0;

        // Get discount (using jQuery since we're already using it)
        const discountElement = $('.order-col').find('div:contains("Coupon Discount")').closest('.order-col');
        const discount = discountElement.length ? parseFloat(discountElement.attr('data-discount')) || 0 : 0;

        // Update shipping display
        const shippingElement = $('.order-col').find('div:contains("Shipping")').closest('.order-col').find('div strong');
        if (shippingElement.length) {
            shippingElement.text(formatCurrency(shippingFee));
        }

        // Calculate and update total
        const newTotal = subtotal - discount + shippingFee;
        const totalElement = document.querySelector('.order-total');
        if (totalElement) {
            totalElement.textContent = formatCurrency(newTotal);
        }
    }

    // Create suggestion item
    function createSuggestionItem(item) {
        const li = document.createElement('li');
        li.className = 'suggestion-item';

        // Create main content div
        const contentDiv = document.createElement('div');
        contentDiv.className = 'suggestion-content';

        // Add address name
        const addressDiv = document.createElement('div');
        addressDiv.className = 'address-name';
        addressDiv.textContent = item.display_name;

        const shippingFee = Number(item.shippingFee) || 0;

        const shippingDiv = document.createElement('div');
        shippingDiv.className = 'shipping-fee text-muted';
        shippingDiv.style.fontSize = '0.9em';
        shippingDiv.textContent = `Phí ship: ${formatCurrency(shippingFee)}`;

        // Append elements
        contentDiv.appendChild(addressDiv);
        contentDiv.appendChild(shippingDiv);
        li.appendChild(contentDiv);

        // Add click handler
        li.addEventListener('click', function () {
            addressInput.value = item.display_name;
            currentShippingFee = shippingFee;
            updateTotal(shippingFee);
            suggestionsList.style.display = 'none';

            // Store shipping info in hidden inputs
            const form = addressInput.closest('form');
            if (form) {
                // Create or update hidden inputs
                let shippingFeeInput = form.querySelector('input[name="ShippingFee"]');
                if (!shippingFeeInput) {
                    shippingFeeInput = document.createElement('input');
                    shippingFeeInput.type = 'hidden';
                    shippingFeeInput.name = 'ShippingFee';
                    form.appendChild(shippingFeeInput);
                }
                shippingFeeInput.value = item.ShippingFee;

                // Store coordinates if needed
                let latInput = form.querySelector('input[name="Lat"]');
                let lonInput = form.querySelector('input[name="Lon"]');
                if (!latInput) {
                    latInput = document.createElement('input');
                    latInput.type = 'hidden';
                    latInput.name = 'Lat';
                    form.appendChild(latInput);
                }
                if (!lonInput) {
                    lonInput = document.createElement('input');
                    lonInput.type = 'hidden';
                    lonInput.name = 'Lon';
                    form.appendChild(lonInput);
                }
                latInput.value = item.Lat;
                lonInput.value = item.Lon;
            }
        });

        return li;
    }

    addressInput.addEventListener('input', function () {
        clearTimeout(debounceTimer);
        const query = this.value.trim();

        if (query.length < 3) {
            suggestionsList.innerHTML = '';
            suggestionsList.style.display = 'none';
            return;
        }

        debounceTimer = setTimeout(() => {
            fetch(`https://localhost:7109/api/Checkout/search?query=${encodeURIComponent(query)}`)                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    suggestionsList.innerHTML = '';

                    if (data && Array.isArray(data) && data.length > 0) {
                        data.forEach(item => {
                            suggestionsList.appendChild(createSuggestionItem(item));
                        });
                        suggestionsList.style.display = 'block';
                    } else {
                        suggestionsList.style.display = 'none';
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    suggestionsList.style.display = 'none';
                });
        }, 300);
    });

    // Handle outside clicks
    document.addEventListener('click', function (e) {
        if (!addressInput.contains(e.target) && !suggestionsList.contains(e.target)) {
            suggestionsList.style.display = 'none';
        }
    });
}
// Initialize all components
document.addEventListener('DOMContentLoaded', function () {
    //setTimeout(() => {
    //    initializePriceFilter();
    //}, 100);
    initializeCategories();
    initAddressAutocomplete();
});
document.addEventListener("DOMContentLoaded", function () {
    // Đợi thêm một chút để đảm bảo noUiSlider đã được khởi tạo
    setTimeout(() => {
        initializePriceFilter();
    }, 100);
});
//Scripts last 14/1/2025
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
            url: '/Cart/RemoveCart', 
            type: 'POST',
            data: { id: productId },
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: "success",
                        title: "Xóa thành công",
                        text: "Đã xóa sản phẩm khỏi danh",
                        showConfirmButton: false,
                        timer: 1000,
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