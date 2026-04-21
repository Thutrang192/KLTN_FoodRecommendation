// ----------------- RATING ---------------------
document.addEventListener('DOMContentLoaded', function () {
    const starRatingContainer = document.querySelector('.star-rating');
    const stars = document.querySelectorAll('.star-item');
    let selectedRating = 0;

    // Kiểm tra an toàn trước khi xử lý Rating
    if (starRatingContainer && stars.length > 0) {
        stars.forEach(star => {
            star.addEventListener('click', function () {
                selectedRating = parseInt(this.getAttribute('data-index'));
                const ratingInput = document.getElementById('RatingValue');
                if (ratingInput) ratingInput.value = selectedRating;
                highlightStars(selectedRating);
            });

            star.addEventListener('mouseenter', function () {
                highlightStars(this.getAttribute('data-index'));
            });
        });

        starRatingContainer.addEventListener('mouseleave', function () {
            highlightStars(selectedRating);
        });
    }

    function highlightStars(index) {
        document.querySelectorAll('.star-item').forEach(s => {
            const i = s.getAttribute('data-index');
            s.classList.toggle('fa-solid', i <= index);
            s.classList.toggle('fa-regular', i > index);
        });
    }

    // Click chọn sao
    const cmtForm = document.getElementById('cmtForm');

    if (cmtForm) {
        cmtForm.addEventListener('submit', function (e) {
            const ratingValue = document.getElementById('RatingValue')?.value;
            const commentInput = document.getElementById('CommentInput')?.value.trim();

            const ratingError = document.getElementById('ratingError');
            const commentError = document.getElementById('commentError');

            let hasError = false;

            // Kiểm tra sao
            if (ratingValue === "0") {
                ratingError?.classList.remove('d-none');
                hasError = true;
            } else {
                ratingError?.classList.add('d-none');
            }

            // Kiểm tra nội dung
            if (commentInput === "") {
                commentError?.classList.remove('d-none');
                hasError = true;
            } else {
                commentError?.classList.add('d-none');
            }

            // Nếu có lỗi, chặn submit
            if (hasError) {
                e.preventDefault();
            }
        });
    } 
})

// ----------------- RelatedRecipes ---------------------
function loadRelatedRecipes(recipeId) {
    $.get('/Home/GetRelatedRecipe/' + recipeId, function (data) {
        $('#related-recipes-container').html(data);
    });
}

// ----------------- REPORT ---------------------
document.addEventListener('DOMContentLoaded', function () {
    const btnReport = document.getElementById('btn-report-trigger');
    const reportModalElement = document.getElementById('reportModal');
    const reportModal = reportModalElement ? new bootstrap.Modal(reportModalElement) : null;

    // Xử lý hiện/ẩn Textarea khi chọn "Lý do khác"
    const checkOther = document.getElementById('reasonOther');
    const otherReasonText = document.getElementById('otherReasonText');

    if (checkOther) {
        checkOther.addEventListener('change', function () {
            if (this.checked) {
                otherReasonText.classList.remove('d-none');
                otherReasonText.focus();
            } else {
                otherReasonText.classList.add('d-none');
                otherReasonText.value = '';
            }
        });
    }

    if (btnReport) {
        btnReport.addEventListener('click', function () {
            const isLoggedIn = this.getAttribute('data-logged-in') === 'true';
            const recipeId = this.getAttribute('data-recipe-id');

            console.log("isLoggedIn", isLoggedIn);
            console.log("recipeId", recipeId);

            if (!isLoggedIn) {
                const currentUrl = window.location.pathname + window.location.search;
                window.location.href = `/Account/Login?ReturnUrl=${encodeURIComponent(currentUrl)}`;
                return;
            }

            if (reportModal) {
                reportModal.show();
            } else {
                console.error("Không tìm thấy modal #reportModal");
            }
        });
    }

    // Xử lý btn Gửi báo cáo
    const btnSend = document.getElementById('btnSendReport');
    if (btnSend) {
        btnSend.addEventListener('click', async function () {
            const checkedBoxes = document.querySelectorAll('.report-checkbox:checked');
            const recipeId = btnReport ? btnReport.getAttribute('data-recipe-id') : null;

            if (checkedBoxes.length === 0) {
                alert("Vui lòng chọn ít nhất một lý do.");
                return;
            }

            let selectedReasons = [];

            checkedBoxes.forEach(cb => {
                if (cb.id === 'reasonOther') {
                    const otherDetail = otherReasonText.value.trim();
                    if (otherDetail) {
                        selectedReasons.push("Khác: " + otherDetail);
                    }
                } else {
                    selectedReasons.push(cb.value);
                }
            });

            // Kiểm tra lại nếu chỉ chọn mỗi "Lý do khác" mà lại để trống textarea
            if (selectedReasons.length === 0) {
                alert("Vui lòng nhập chi tiết cho lý do khác.");
                otherReasonText.focus();
                return;
            }

            // Gộp mảng thành chuỗi: "Lý do 1, Lý do 2, Khác: abc"
            const finalReason = selectedReasons.join(', ');

            // Hiệu ứng loading (optional)
            btnSend.disabled = true;
            btnSend.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang gửi...';

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            try {
                const response = await fetch('/Home/ReportRecipe', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': token || ''
                    },
                    body: new URLSearchParams({
                        'recipeId': recipeId,
                        'reason': finalReason
                    })
                });

                const result = await response.json();

                if (result.success) {
                    const container = document.getElementById('report-container');
                    container.innerHTML = `
                        <button class="px-4 py-2 flex items-center gap-2 btn btn-outline-orange fw-medium" disabled>
                        <i class="fa-solid fa-flag"></i> Đã báo cáo món này
                        </button>`;

                    reportModal.hide();
                }
                else {
                    alert(result.message || "Có lỗi xảy ra.");
                }
            } catch (error) {
                console.error("Lỗi: ", error);
            } finally {
                btnSend.disabled = false;
                btnSend.innerHTML = 'Gửi báo cáo';
            }
        });
    }
});

