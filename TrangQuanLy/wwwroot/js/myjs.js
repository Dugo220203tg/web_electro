// admin.js

// Constants
const API_BASE_URL = 'https://localhost:7109/api';

// Order Status Management
function attachEventListeners() {
    console.log('Attempting to attach event listeners');

    const statusSelects = document.querySelectorAll('select[id^="selectTrangThai-"]');
    console.log('Found ' + statusSelects.length + ' select elements');

    statusSelects.forEach(select => {
        console.log('Attaching listener to', select.id);
        select.addEventListener('change', function () {
            console.log('Change event triggered for', this.id);
            const maHD = this.id.split('-')[1];
            confirmChangeTrangThai(maHD, this);
        });
    });
}

function confirmChangeTrangThai(maHD, selectElement) {
    console.log('confirmChangeTrangThai called for order:', maHD);

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

    // Add form submit handler for image names
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function (event) {
            ImageHandler.updateExistingImageNames();
        });
    }
});

// Expose necessary functions to window object for inline event handlers
window.ImageHandler = ImageHandler;

// Export functions and objects for use in other files
export {
    ImageHandler,
    DateHandler,
    UIUtils,
    initializeFormHandlers
};