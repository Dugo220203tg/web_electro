// admin.js

// Constants
const API_BASE_URL = 'https://localhost:7109/api';

// Order Status Management
function attachEventListeners() {
    const statusSelects = document.querySelectorAll('select[id^="selectTrangThai-"]');

    statusSelects.forEach(select => {
        select.addEventListener('change', function () {
            console.log('Change event triggered for', this.id);
            const maHD = this.id.split('-')[1];
            confirmChangeTrangThai(maHD, this);
        });
    });
}

function confirmChangeTrangThai(maHD, selectElement) {
    const newMaTrangThai = selectElement.value;
    const originalValue = selectElement.getAttribute('data-original-value');

    if (confirm("Bạn có chắc chắn muốn thay đổi trạng thái của đơn hàng này không?")) {
        console.log('User confirmed status change. Sending AJAX request...');

        $.ajax({
            url: `${API_BASE_URL}/HoaDon/UpdateTrangThai`,
            type: 'POST',
            data: JSON.stringify({
                maHD: maHD,
                maTrangThai: newMaTrangThai
            }),
            contentType: 'application/json',
            success: function (response) {
                console.log('AJAX request successful. Status updated.');
                alert('Cập nhật trạng thái thành công!');
                selectElement.setAttribute('data-original-value', newMaTrangThai);
            },
            error: function (xhr) {
                console.error('AJAX request failed:', xhr.statusText);
                alert('Cập nhật trạng thái thất bại: ' + xhr.responseText);
                selectElement.value = originalValue;
            }
        });
    } else {
        console.log('User cancelled status change. Reverting select value.');
        selectElement.value = originalValue;
    }
}

// Sales Chart
function initializeSalesChart(labels, salesData1, salesData8, salesData11) {
    Chart.defaults.color = "#6C7293";
    Chart.defaults.borderColor = "#000000";
    const ctx = document.getElementById('worldwide-sales').getContext('2d');
    new Chart(ctx, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Linh kiện',
                    data: salesData1,
                    backgroundColor: "rgba(235, 22, 22, .4)"
                },
                {
                    label: 'Màn hình',
                    data: salesData8,
                    backgroundColor: "rgba(235, 22, 22, .6)"
                },
                {
                    label: 'Laptop',
                    data: salesData11,
                    backgroundColor: "rgba(235, 22, 22, .8)"
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    min: 0,
                    // Điều chỉnh max tự động dựa trên dữ liệu
                    suggestedMax: Math.max(
                        ...salesData1,
                        ...salesData8,
                        ...salesData11
                    ) * 1.1,
                    ticks: {
                        stepSize: 5
                    },
                    title: {
                        display: true,
                        text: 'Số lượng bán ra'
                    }
                }
            },
            plugins: {
                legend: {
                    position: 'top',
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return context.dataset.label + ': ' + context.parsed.y + ' sản phẩm';
                        }
                    }
                }
            }
        }
    });
}
// Account Management
function toggleAccountStatus(username, currentStatus) {
    currentStatus = (currentStatus === 'true' || currentStatus === true);

    $.ajax({
        url: `${API_BASE_URL}/KhachHangs/UpdateTrangThai`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            UserName: username,
            HieuLuc: !currentStatus
        }),
        success: function (response) {
            const button = $("#status-button-" + username);
            const statusText = $("#status-text-" + username + " span");

            if (currentStatus) {
                button.val("Mở tài khoản");
                statusText.text("Không hoạt động");
            } else {
                button.val("Khóa tài khoản");
                statusText.text("Đang hoạt động");
            }

            button.attr("onclick", `toggleAccountStatus('${username}', ${!currentStatus})`);
            alert(response.message || "Cập nhật trạng thái thành công!");
        },
        error: function (xhr, status, error) {
            console.error("Error:", error);
            alert("Đã có lỗi xảy ra: " + (xhr.responseText || "Không thể cập nhật trạng thái"));
        }
    });
}

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    attachEventListeners();
    // Note: Sales chart initialization should be called with data from your backend
});

// Export functions for use in other files if needed
export {
    attachEventListeners,
    confirmChangeTrangThai,
    initializeSalesChart,
    toggleAccountStatus
};
// Form and Select Management
function initializeFormHandlers() {
    // Set selected value for select elements
    function setSelectedValue(selectId, inputId) {
        const selectElement = document.getElementById(selectId);
        const inputValueElement = document.getElementById(inputId);

        if (selectElement && inputValueElement) {
            const inputValue = inputValueElement.value;
            if (inputValue) {
                for (let i = 0; i < selectElement.options.length; i++) {
                    if (selectElement.options[i].value === inputValue) {
                        selectElement.selectedIndex = i;
                        break;
                    }
                }
            }
        }
    }

    // Initialize select elements
    function initializeSelects() {
        setSelectedValue('selectNCC', 'inputValueNCC');
        setSelectedValue('selectLoaiSp', 'inputValueLoaiSp');

        // Add change event listeners
        const selectLoaiSp = document.getElementById('selectLoaiSp');
        const selectNCC = document.getElementById('selectNCC');

        if (selectLoaiSp) {
            selectLoaiSp.addEventListener('change', function () {
                const inputValue = document.getElementById('inputValueLoaiSp');
                if (inputValue) inputValue.value = this.value || '';
            });
        }

        if (selectNCC) {
            selectNCC.addEventListener('change', function () {
                const inputValue = document.getElementById('inputValueNCC');
                if (inputValue) inputValue.value = this.value || '';
            });
        }
    }

    initializeSelects();
}

// Image Handling
const ImageHandler = {
    previewImage: function (input) {
        if (input.files && input.files[0]) {
            const reader = new FileReader();
            reader.onload = function (e) {
                const previewItem = input.closest('.image-input-group').querySelector('.image-preview-item');
                previewItem.innerHTML = '';

                const img = document.createElement('img');
                img.src = e.target.result;
                img.className = 'preview-image';
                img.alt = 'Preview';

                const p = document.createElement('p');
                p.className = 'image-name';
                p.textContent = input.files[0].name;

                previewItem.appendChild(img);
                previewItem.appendChild(p);

                ImageHandler.updateExistingImageNames();
            };
            reader.readAsDataURL(input.files[0]);
        }
    },

    addNewImageInput: function () {
        const container = document.getElementById('imageInputsContainer');
        if (container) {
            const newIndex = container.children.length;
            const newGroup = document.createElement('div');
            newGroup.className = 'image-input-group mb-3';
            newGroup.innerHTML = `
                <div class="image-preview-item"></div>
                <div class="input-group input-group-img">
                    <input type="file" class="form-control bg-dark image-input" 
                           name="ImageFiles" onchange="window.ImageHandler.previewImage(this)" 
                           data-index="${newIndex}">
                    <button type="button" class="btn btn-danger remove-image" 
                            onclick="window.ImageHandler.removeImageInput(this)">Xóa</button>
                </div>`;
            container.appendChild(newGroup);
        }
    },

    removeImageInput: function (button) {
        const group = button.closest('.image-input-group');
        if (group) {
            const img = group.querySelector('.preview-image');
            if (img && img.getAttribute('data-existing') === 'true') {
                const existingImages = document.getElementById('hinhInput').value.split(',');
                const updatedImages = existingImages.filter(name => name !== img.src.split('/').pop());
                document.getElementById('hinhInput').value = updatedImages.join(',');
            }
            group.remove();
            this.updateExistingImageNames();
        }
    },

    updateExistingImageNames: function () {
        const oldImageNames = Array.from(document.querySelectorAll('.image-preview-item img'))
            .map(img => img.src.split('/').pop()); // Lấy tên file từ URL
        const newImageNames = Array.from(document.querySelectorAll('.image-name'))
            .map(el => el.textContent);
        const combinedImageNames = [...oldImageNames, ...newImageNames];

        const hinhInput = document.getElementById('hinhInput');
        if (hinhInput) {
            hinhInput.value = combinedImageNames.join(',');
        }
    },

    initialize: function () {
        const imageInputsContainer = document.getElementById('imageInputsContainer');
        if (imageInputsContainer && imageInputsContainer.children.length === 0) {
            this.addNewImageInput();
        }
        this.updateExistingImageNames();
    }
};

// Date Handling
const DateHandler = {
    initializeDatepickers: function () {
        if (typeof $ !== 'undefined') {
            if ($.fn.datepicker) {
                $('.datepicker').datepicker({
                    format: 'dd/mm/yyyy',
                    autoclose: true,
                    todayHighlight: true
                });
            }

            if ($.fn.datetimepicker) {
                $(".date").datetimepicker({
                    format: 'YYYY-MM-DD',
                    icons: {
                        time: "fa fa-clock-o",
                        date: "fa fa-calendar",
                        up: "fa fa-arrow-up",
                        down: "fa fa-arrow-down"
                    },
                    showTodayButton: true,
                    useCurrent: false
                }).find('input:first').on("blur", function () {
                    const date = DateHandler.parseDate($(this).val());
                    $(this).val(DateHandler.isValidDate(date) ?
                        date : moment().format('YYYY-MM-DD'));
                });
            }
        }
    },

    isValidDate: function (value) {
        return !isNaN(Date.parse(value));
    },

    parseDate: function (value) {
        const m = value.match(/^(\d{1,2})(\/|-)?(\d{1,2})(\/|-)?(\d{4})$/);
        return m ? `${m[5]}-${("00" + m[3]).slice(-2)}-${("00" + m[1]).slice(-2)}` : value;
    }
};

// UI Utilities
const UIUtils = {
    initializeDeleteConfirmation: function () {
        const deleteForm = document.getElementById('delete-form');
        if (deleteForm) {
            deleteForm.addEventListener('submit', function (event) {
                if (!confirm('Bạn có chắc muốn xóa?')) {
                    event.preventDefault();
                }
            });
        }
    },

};

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    initializeFormHandlers();
    ImageHandler.initialize();
    DateHandler.initializeDatepickers();
    UIUtils.initializeDeleteConfirmation();
    //initializeNotifications();
    initializeDataTable();
    initializeCarousel();
    // Add form submit handler for image names
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function (event) {
            ImageHandler.updateExistingImageNames();
        });
    }
});

window.ImageHandler = ImageHandler;

// Export functions and objects for use in other files
export {
    ImageHandler,
    DateHandler,
    UIUtils,
    initializeFormHandlers
};

//Date js 16/1/2025
function initializeNotifications() {
    // Lấy message từ hidden field thay vì trực tiếp từ Razor
    var successMessage = document.getElementById('successMessage')?.value;
    var errorMessage = document.getElementById('errorMessage')?.value;

    if (successMessage && successMessage.trim()) {
        Swal.fire({
            icon: 'success',
            title: 'Thông báo',
            text: successMessage
        });
    } else if (errorMessage && errorMessage.trim()) {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: errorMessage
        });
    }
}
function showNotification(type, message) {
    Swal.fire({
        icon: type,
        title: type === 'success' ? 'Thông báo' : 'Lỗi',
        text: message
    });
}

// comments.js
function toggleCommentStatus(maDg, currentStatus) {
    const newStatus = currentStatus === 1 ? 0 : 1;
    const button = document.getElementById(`status-button-${maDg}`);
    const statusText = document.getElementById(`status-text-${maDg}`);

    $.ajax({
        type: "POST",
        url: 'https://localhost:7109/api/DanhGiaSp/UpdateTrangThai',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ MaDg: maDg, TrangThai: newStatus }),
        success: function (response) {
            alert(response.message);
            button.value = newStatus === 1 ? "Ẩn đánh giá" : "Hiển thị đánh giá";
            button.setAttribute("onclick", `toggleCommentStatus('${maDg}', ${newStatus})`);
            statusText.textContent = newStatus === 1 ? "Hiển Thị" : "Đã Ẩn";
        },
        error: function (xhr) {
            alert("Có lỗi xảy ra: " + xhr.responseText);
        }
    });
}

// datatable.js
function initializeDataTable() {
    $('#myTable').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/vi.json',
        },
        paging: false,
        ordering: true,
        searching: false,
        info: false,
        responsive: true,
        autoWidth: false,
        columnDefs: [{
            targets: -1,
            orderable: false,
            searchable: false
        }],
        scrollY: '50vh',
        scrollCollapse: true,
    });
}

// carousel.js
function initializeCarousel() {
    // Owl Carousel initialization
    $(".testimonial-carousel").owlCarousel({
        items: 1,
        loop: true,
        margin: 10,
        nav: true,
        dots: true,
        navText: ["<div class='owl-prev'>‹</div>", "<div class='owl-next'>›</div>"]
    });

    // Custom carousel functionality
    const items = document.querySelectorAll('.testimonial-item');
    const dots = document.querySelectorAll('.owl-dot');
    let currentIndex = 0;
    const autoPlayDelay = 5000;
    let autoPlayInterval;

    function showItem(index) {
        items.forEach(item => {
            item.classList.remove('active', 'prev', 'next');
        });
        dots.forEach(dot => dot.classList.remove('active'));

        items[currentIndex].classList.remove('active');
        items[currentIndex].classList.add('prev');

        currentIndex = index;

        items[currentIndex].classList.add('active');
        dots[currentIndex].classList.add('active');

        resetAutoPlay();
    }

    function nextItem() {
        const nextIndex = (currentIndex + 1) % items.length;
        showItem(nextIndex);
    }

    function resetAutoPlay() {
        clearInterval(autoPlayInterval);
        autoPlayInterval = setInterval(nextItem, autoPlayDelay);
    }

    dots.forEach(dot => {
        dot.addEventListener('click', function () {
            const index = parseInt(this.getAttribute('data-index'));
            showItem(index);
        });
    });

    resetAutoPlay();

    const carousel = document.querySelector('.owl-carousel');
    carousel.addEventListener('mouseenter', () => clearInterval(autoPlayInterval));
    carousel.addEventListener('mouseleave', resetAutoPlay);
}

// order.js
const orderUtils = {
    removeRow: function (button) {
        const row = button.closest('tr');
        if (!row) return;

        const tbody = row.closest('tbody');
        const rows = Array.from(tbody.querySelectorAll('tr.product-widget'));
        const index = rows.indexOf(row);

        const form = row.closest('form');
        const hiddenDelete = document.createElement('input');
        hiddenDelete.type = 'hidden';
        hiddenDelete.name = `ChiTietHds[${index}].IsDeleted`;
        hiddenDelete.value = 'true';
        form.appendChild(hiddenDelete);

        row.style.display = 'none';
        this.updateTotal();
    },

    updateTotal: function () {
        let total = 0;
        const rows = document.querySelectorAll('tr.product-widget');
        const shippingFee = parseFloat(document.querySelector('input[name="PhiVanChuyen"]').value) || 0;

        rows.forEach(row => {
            if (row.style.display !== 'none') {
                const quantity = parseFloat(row.querySelector('input[type="number"]').value) || 0;
                const price = parseFloat(row.querySelector('input[type="text"]').value) || 0;
                total += quantity * price;
            }
        });

        total += shippingFee;
        const formattedTotal = new Intl.NumberFormat('vi-VN').format(total);
        document.querySelector('.order-total').textContent = `${formattedTotal} VND`;
    }
};

// product.js
function confirmDelete(url, data) {
    console.log('Starting delete with:', { url, data }); // Log dữ liệu đầu vào
    try {
        Swal.fire({
            title: 'Bạn có chắc chắn muốn xóa?',
            text: "Dữ liệu sẽ không thể khôi phục sau khi xóa!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                console.log('User confirmed delete');
                $.ajax({
                    url: url,
                    type: 'POST',
                    data: data,
                    beforeSend: function () {
                        console.log('Sending request to:', url);
                        console.log('With data:', data);
                    },
                    success: function (response) {
                        console.log('Delete response:', response);
                        if (response.success) {
                            Swal.fire(
                                'Đã xóa!',
                                'Xóa dữ liệu thành công.',
                                'success'
                            ).then(() => {
                                location.reload();
                            });
                        } else {
                            console.error('Delete failed:', response);
                            Swal.fire(
                                'Lỗi!',
                                'Xóa không thành công: ' + (response.message || 'Không có thông tin lỗi'),
                                'error'
                            );
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('Delete error details:', {
                            xhr: xhr,
                            status: status,
                            error: error,
                            responseText: xhr.responseText
                        });
                        Swal.fire(
                            'Lỗi!',
                            'Lỗi khi xóa: ' + error,
                            'error'
                        );
                    }
                });
            }
        });
    } catch (error) {
        console.error('Error in confirmDelete:', error);
    }
}

// Đảm bảo functions có sẵn trong window object
window.confirmDelete = confirmDelete;
window.initializeNotifications = initializeNotifications;
window.initializeDataTable = initializeDataTable;

document.addEventListener("DOMContentLoaded", function () {
    const notificationToggle = document.getElementById("notification-toggle");
    const notificationContainer = document.getElementById("notification-container");

    // Gọi API đánh dấu tất cả thông báo là đã đọc
    notificationToggle.addEventListener("click", async function () {
        try {
            const response = await fetch('https://localhost:7109/api/Notification/MarkNotificationsAsSeen', {
                method: 'POST'
            });

            if (response.ok) {
                console.log("All notifications marked as seen.");
                // Tải lại danh sách thông báo sau khi đánh dấu
                loadNotifications();
            } else {
                console.error("Failed to mark notifications as seen.");
            }
        } catch (error) {
            console.error("Error:", error);
        }
    });

    // Hàm tải lại danh sách thông báo
    async function loadNotifications() {
        try {
            const response = await fetch('https://localhost:7109/api/Notification/GetUnseenNotifications');
            if (response.ok) {
                const data = await response.text(); // Dữ liệu là HTML từ ViewComponent
                notificationContainer.innerHTML = data;
            } else {
                console.error("Failed to load notifications.");
            }
        } catch (error) {
            console.error("Error:", error);
        }
    }
});
