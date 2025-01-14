    <script>
        $(function () {
            var message = '@Html.Raw(TempData["success"])';
            var errorMessage = '@Html.Raw(TempData["Error"])';

            if (message) {
                Swal.fire({
                    icon: 'success',
                    title: 'Thông báo',
                    text: message
                });
            } else if (errorMessage) {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: errorMessage
                });
            }
        });
    </script> 
< !--Giới hạn hiển thị số lượng phần lọc theo loại và danh mục sản phẩm ở page _DanhSachHangHoa-- >
    <script>
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
    </script>
    <!--jQuery Lọc giá sản hàng hóa-- >
    <script>
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
    </script>

    <!--Huy Don Hang trang Don Hang-- >
    <script type="text/javascript">
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
    </script>
    <!--Script đánh giá sản phẩm trong detail-- >
    <script>
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

        // Initialize tooltips and other UI elements if needed
        $(document).ready(function () {
            // Any initialization code here
        });
    </script>
    <script>
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

    </script>
    <!--Danh mục sản phẩm trong trang store-- >
    <script>
        // Function to get query parameters from the URL
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
    </script>
    <!--Trang đơn hàng xử lý hiển thị-- >
    @* <script>
        $(document).ready(function () {
            // Handle View button click
            $(".view-order").click(function () {
                const orderId = $(this).data("id"); // Get the order ID

                // Hide all orders and remove 'active' class
                $(".order-details-nb").removeClass("active").addClass("d-none");

                // Show the clicked order
                $(`.order-details-nb[data-id="${orderId}"]`).removeClass("d-none").addClass("active");
            });
    });
    </script> * @
    < script >
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
    </script >

    < !--Check giong khac hang trong checkout-- >
    <script>
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
    </script>
    <!--Xóa sản phẩm trong cart-- >
    <script>
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
                            text: "Đã xóa sản phẩm khỏi danh sách",
                            showConfirmButton: false,
                            timer: 1000,
                        }).then(() => {
                            // Remove the closest product widget
                            $(buttonElement).closest(".product-widget").remove();
                            // Update the cart display
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

    </script>
    <!--OpenStreetMap -->
    <script>
        // Đợi DOM load xong mới chạy script
        function initAddressAutocomplete() {
            const addressInput = document.getElementById('address-input');
        const suggestionsList = document.getElementById('suggestions-list');

        // Kiểm tra xem elements có tồn tại không
        if (!addressInput || !suggestionsList) {
                // console.error('Required elements not found. Check IDs: address-input and suggestions-list');
                return;
            }

        let debounceTimer;

        addressInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
        const query = this.value.trim();

        console.log('Input value:', query);

        if (query.length < 3) {
            suggestionsList.innerHTML = '';
        return;
                }

        const apiUrl = `/Checkout/search?query=${encodeURIComponent(query)}`;
            };
        console.log('Calling API:', apiUrl);

            debounceTimer = setTimeout(() => {
            fetch(apiUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(response => {
                    console.log('Response status:', response.status);
                    if (!response.ok) {
                        throw new Error(HTTP error! status: ${response.status});
                        }
        return response.json();
                    })
                    .then(data => {
            console.log('API Response:', data);
        suggestionsList.innerHTML = '';

        if (data && Array.isArray(data)) {
            data.forEach(item => {
                const li = document.createElement('li');
                li.className = 'suggestion-item';
                li.textContent = item.display_name;
                li.addEventListener('click', function () {
                    addressInput.value = item.display_name;
                    suggestionsList.style.display = 'none';
                });
                suggestionsList.appendChild(li);
            });

                            if (data.length > 0) {
            suggestionsList.style.display = 'block';
                            }
                        }
                    })
                    .catch(error => {
            console.error('Error:', error);
        Swal.fire({
            icon: "error",
        title: "Lỗi",
        text: "Đã xảy ra lỗi khi tìm kiếm địa chỉ",
                        });
                    });
            }, 300);
        });

        // Xử lý click outside
        document.addEventListener('click', function (e) {
            if (!addressInput.contains(e.target) && !suggestionsList.contains(e.target)) {
            suggestionsList.style.display = 'none';
            }
        });
                    }

        // Có thể sử dụng một trong hai cách sau để khởi tạo:

        // Cách 1: Sử dụng DOMContentLoaded
        document.addEventListener('DOMContentLoaded', initAddressAutocomplete);

    // Cách 2: Gọi hàm sau khi load page
    // window.onload = initAddressAutocomplete;
    </script>