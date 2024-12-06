document.addEventListener("DOMContentLoaded", function () {
    const boxes = document.querySelectorAll(".info-box");

    boxes.forEach((box, index) => {
        setTimeout(() => {
            if (index % 2 === 0) {
                box.classList.add("slide-in-left");
            } else {
                box.classList.add("slide-in-right");
            }
        }, index * 300); // Delay each box's animation
    });
});