// Initialize Swiper carousels
document.addEventListener('DOMContentLoaded', function() {
    if (typeof Swiper === 'undefined') {
        console.error('Swiper is not loaded');
        return;
    }
    initSwiperCarousels();
});

function initSwiperCarousels() {
    const swiperContainers = document.querySelectorAll('.swiper-container');

    swiperContainers.forEach(function(swiperContainer) {
        // Get all data attributes
        const speed = parseInt(swiperContainer.dataset.speed) || 400;
        const spaceBetween = parseInt(swiperContainer.dataset.spaceBetween) || 20;
        const paginationEnabled = swiperContainer.dataset.pagination === 'true';
        const navigationEnabled = swiperContainer.dataset.navigation === 'true';
        const autoplayEnabled = swiperContainer.dataset.autoplay === 'true';
        const autoplayDelay = parseInt(swiperContainer.dataset.autoplayDelay) || 3000;
        const paginationType = swiperContainer.dataset.paginationType || 'bullets';
        const effect = swiperContainer.dataset.effect || 'slide';
        const centerSlides = swiperContainer.dataset.centerSlides === 'true';

        // Parse breakpoints
        let breakpoints = {};
        const breakpointsData = swiperContainer.dataset.breakpoints;
        if (breakpointsData) {
            try {
                breakpoints = JSON.parse(breakpointsData);
            } catch (e) {
                console.error('Error parsing breakpoints:', e);
            }
        }

        // Build swiper options
        const swiperOptions = {
            speed: speed,
            effect: effect,
            loop: false,
            spaceBetween: spaceBetween,
            breakpoints: breakpoints,
            slidesPerView: 'auto'
        };

        if (centerSlides) {
            swiperOptions.centeredSlides = true;
        }

        // Pagination
        if (paginationEnabled) {
            const paginationEl = swiperContainer.querySelector('.swiper-pagination');
            if (paginationEl) {
                swiperOptions.pagination = {
                    el: paginationEl,
                    clickable: true,
                    type: paginationType
                };
            }
        }

        // Navigation (arrows) - look inside swiper-navigation wrapper
        if (navigationEnabled) {
            const navWrapper = swiperContainer.querySelector('.swiper-navigation');
            if (navWrapper) {
                const nextBtn = navWrapper.querySelector('.swiper-button-next');
                const prevBtn = navWrapper.querySelector('.swiper-button-prev');
                if (nextBtn || prevBtn) {
                    swiperOptions.navigation = {
                        nextEl: nextBtn,
                        prevEl: prevBtn
                    };
                }
            }
        }

        // Autoplay
        if (autoplayEnabled) {
            swiperOptions.autoplay = {
                delay: autoplayDelay,
                disableOnInteraction: false,
                pauseOnMouseEnter: true
            };
        }

        // Destroy existing instance if any
        if (swiperContainer.swiper) {
            swiperContainer.swiper.destroy(true, true);
        }

        // Initialize new Swiper instance
        new Swiper(swiperContainer, swiperOptions);
    });
}