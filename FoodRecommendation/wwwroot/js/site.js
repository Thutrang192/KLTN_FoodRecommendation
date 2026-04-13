let selectedRating = 0;

// 1. Logic chọn sao
document.querySelectorAll('.star-item').forEach(star => {
    star.addEventListener('click', function () {
        selectedRating = this.getAttribute('data-index');
        document.getElementById('RatingValue').value = selectedRating;

        // Tô màu các sao đã chọn
        highlightStars(selectedRating);
    });

    star.addEventListener('mouseenter', function () {
        highlightStars(this.getAttribute('data-index'));
    });
});

document.querySelector('.star-rating').addEventListener('mouseleave', function () {
    highlightStars(selectedRating);
});

function highlightStars(index) {
    document.querySelectorAll('.star-item').forEach(s => {
        const i = s.getAttribute('data-index');
        s.classList.toggle('fa-solid', i <= index);
        s.classList.toggle('fa-regular', i > index);
    });
}

// 2. Kiểm tra rỗng khi Submit
document.getElementById('cmtForm').addEventListener('submit', function (e) {
    const ratingValue = document.getElementById('RatingValue').value;
    const commentInput = document.getElementById('CommentInput').value.trim();

    const ratingError = document.getElementById('ratingError');
    const commentError = document.getElementById('commentError');

    let hasError = false;

    // Kiểm tra sao
    if (ratingValue === "0") {
        ratingError.classList.remove('d-none');
        hasError = true;
    } else {
        ratingError.classList.add('d-none');    
    }

    // Kiểm tra nội dung
    if (commentInput === "") {
        commentError.classList.remove('d-none');
        hasError = true;
    } else {
        commentError.classList.add('d-none');   
    }

    // Nếu có bất kỳ lỗi nào, chặn không cho load lại trang
    if (hasError) {
        e.preventDefault();
    }
});