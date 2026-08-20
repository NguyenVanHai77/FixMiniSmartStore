// ===== LƯU Ý: XỬ LÝ TRANG HOME =====

document.addEventListener(
    "DOMContentLoaded",
    function () {

        // =====================================
        // POPUP KHUYẾN MÃI
        // =====================================

        const promotionPopup =
            document.getElementById(
                "homePromotionPopup"
            );

        const closePromotionButton =
            document.getElementById(
                "closeHomePromotion"
            );


        if (promotionPopup) {

            document.body.style.overflow =
                "hidden";


            function closePromotion() {

                promotionPopup
                    .classList
                    .remove("show");

                document.body.style.overflow =
                    "";

            }


            if (closePromotionButton) {

                closePromotionButton
                    .addEventListener(
                        "click",
                        closePromotion
                    );

            }


            promotionPopup.addEventListener(
                "click",
                function (event) {

                    if (
                        event.target ===
                        promotionPopup
                    ) {

                        closePromotion();

                    }

                }
            );


            document.addEventListener(
                "keydown",
                function (event) {

                    if (
                        event.key ===
                        "Escape"
                    ) {

                        closePromotion();

                    }

                }
            );

        }



        // =====================================
        // SLIDER BANNER TRANG CHỦ
        // =====================================

        const slider =
            document.getElementById(
                "homeHeroSlider"
            );

        const track =
            document.getElementById(
                "homeHeroTrack"
            );

        const previousButton =
            document.getElementById(
                "homeSliderPrev"
            );

        const nextButton =
            document.getElementById(
                "homeSliderNext"
            );


        if (!slider || !track) {

            return;

        }


        const slides =
            Array.from(
                track.querySelectorAll(
                    ".home-hero-slide"
                )
            );

        const dots =
            Array.from(
                slider.querySelectorAll(
                    ".home-slider-dot"
                )
            );


        if (slides.length === 0) {

            return;

        }


        let currentSlide = 0;

        let autoSlideTimer = null;



        // =====================================
        // CẬP NHẬT VỊ TRÍ SLIDER
        // =====================================

        function updateSlider() {

            track.style.transform =
                `translateX(-${currentSlide * 100
                }%)`;


            dots.forEach(
                function (dot, index) {

                    const isActive =
                        index === currentSlide;


                    dot.classList.toggle(
                        "active",
                        isActive
                    );


                    if (isActive) {

                        dot.setAttribute(
                            "aria-current",
                            "true"
                        );

                    }
                    else {

                        dot.removeAttribute(
                            "aria-current"
                        );

                    }

                }
            );

        }



        // =====================================
        // CHUYỂN ĐẾN SLIDE
        // =====================================

        function goToSlide(index) {

            if (index < 0) {

                currentSlide =
                    slides.length - 1;

            }
            else if (
                index >= slides.length
            ) {

                currentSlide = 0;

            }
            else {

                currentSlide = index;

            }


            updateSlider();

        }



        // =====================================
        // DỪNG SLIDER TỰ ĐỘNG
        // =====================================

        function stopAutoSlide() {

            if (autoSlideTimer !== null) {

                clearInterval(
                    autoSlideTimer
                );

                autoSlideTimer = null;

            }

        }



        // =====================================
        // TỰ CHUYỂN SAU 5 GIÂY
        // =====================================

        function startAutoSlide() {

            stopAutoSlide();


            autoSlideTimer =
                setInterval(
                    function () {

                        goToSlide(
                            currentSlide + 1
                        );

                    },
                    5000
                );

        }



        // =====================================
        // MŨI TÊN TRÁI
        // =====================================

        if (previousButton) {

            previousButton
                .addEventListener(
                    "click",
                    function () {

                        goToSlide(
                            currentSlide - 1
                        );

                        startAutoSlide();

                    }
                );

        }



        // =====================================
        // MŨI TÊN PHẢI
        // =====================================

        if (nextButton) {

            nextButton
                .addEventListener(
                    "click",
                    function () {

                        goToSlide(
                            currentSlide + 1
                        );

                        startAutoSlide();

                    }
                );

        }



        // =====================================
        // CHẤM ĐIỀU HƯỚNG
        // =====================================

        dots.forEach(
            function (dot) {

                dot.addEventListener(
                    "click",
                    function () {

                        const index =
                            Number(
                                dot.dataset.slide
                            );


                        if (
                            Number.isNaN(index)
                        ) {

                            return;

                        }


                        goToSlide(
                            index
                        );

                        startAutoSlide();

                    }
                );

            }
        );



        // =====================================
        // HOVER THÌ TẠM DỪNG
        // =====================================

        slider.addEventListener(
            "mouseenter",
            function () {

                stopAutoSlide();

            }
        );


        slider.addEventListener(
            "mouseleave",
            function () {

                startAutoSlide();

            }
        );



        // =====================================
        // PHÍM MŨI TÊN
        // =====================================

        slider.setAttribute(
            "tabindex",
            "0"
        );


        slider.addEventListener(
            "keydown",
            function (event) {

                if (
                    event.key ===
                    "ArrowLeft"
                ) {

                    goToSlide(
                        currentSlide - 1
                    );

                    startAutoSlide();

                }


                if (
                    event.key ===
                    "ArrowRight"
                ) {

                    goToSlide(
                        currentSlide + 1
                    );

                    startAutoSlide();

                }

            }
        );



        // =====================================
        // TẠM DỪNG KHI ĐỔI TAB
        // =====================================

        document.addEventListener(
            "visibilitychange",
            function () {

                if (document.hidden) {

                    stopAutoSlide();

                }
                else {

                    startAutoSlide();

                }

            }
        );



        // =====================================
        // KHỞI ĐỘNG
        // =====================================

        updateSlider();

        startAutoSlide();

    }
);

// ===== KẾT THÚC XỬ LÝ TRANG HOME =====