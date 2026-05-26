// Custom JS for Furnish E-commerce
document.addEventListener('DOMContentLoaded', () => {
    console.log('Furnish - Premium Furniture Store');

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Add to cart notification
    const addToCartForms = document.querySelectorAll('form[action*="Cart/Add"]');
    addToCartForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const button = form.querySelector('button[type="submit"]');
            if (button) {
                const originalText = button.innerHTML;
                button.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Adding...';
                button.disabled = true;

                setTimeout(() => {
                    button.innerHTML = '<i class="bi bi-check-lg me-2"></i>Added!';
                    button.classList.remove('btn-primary');
                    button.classList.add('btn-success');

                    setTimeout(() => {
                        button.innerHTML = originalText;
                        button.classList.remove('btn-success');
                        button.classList.add('btn-primary');
                        button.disabled = false;
                    }, 1500);
                }, 500);
            }
        });
    });

    // Quantity update buttons
    window.updateQuantity = function(button, productId, variantId, change) {
        const input = button.closest('.input-group').querySelector('input[name="quantity"]');
        let value = parseInt(input.value, 10) || 1;
        value = value + change;
        if (value < 1) value = 1;
        if (value > 99) value = 99;
        input.value = value;

        // Auto-submit the form
        const form = button.closest('form');
        if (form) {
            form.submit();
        }
    };

    // Remove from cart confirmation
    const removeForms = document.querySelectorAll('form[action*="Remove"]');
    removeForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            if (!confirm('Remove this item from your cart?')) {
                e.preventDefault();
            }
        });
    });

    // Clear cart confirmation
    const clearForms = document.querySelectorAll('form[action*="Clear"]');
    clearForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            if (!confirm('Clear all items from your cart?')) {
                e.preventDefault();
            }
        });
    });

    // Payment method selection visual feedback
    const paymentCards = document.querySelectorAll('.payment-card');
    paymentCards.forEach(card => {
        card.addEventListener('click', function() {
            paymentCards.forEach(c => c.classList.remove('border-primary', 'border-2'));
            this.classList.add('border-primary', 'border-2');
            const radio = this.querySelector('input[type="radio"]');
            if (radio) radio.checked = true;
        });
    });

    // Wishlist add animation
    const wishlistForms = document.querySelectorAll('form[action*="Wishlist/Add"]');
    wishlistForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const button = form.querySelector('button[type="submit"]');
            if (button && !button.classList.contains('active')) {
                e.preventDefault();

                button.innerHTML = '<i class="bi bi-heart-fill me-2"></i>Added!';
                button.classList.add('active', 'btn-danger');

                // Submit after animation
                setTimeout(() => {
                    form.submit();
                }, 1000);
            }
        });
    });

    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            if (href !== '#' && href.length > 1) {
                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            }
        });
    });

    // Form validation enhancement
    const forms = document.querySelectorAll('form.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // Lazy load images with fade-in effect
    const lazyImages = document.querySelectorAll('img[data-src]');
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.add('loaded');
                    imageObserver.unobserve(img);
                }
            });
        });

        lazyImages.forEach(img => imageObserver.observe(img));
    }

    // Product card hover effect enhancement
    const productCards = document.querySelectorAll('.product-card');
    productCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.classList.add('hovered');
        });
        card.addEventListener('mouseleave', function() {
            this.classList.remove('hovered');
        });
    });

    // Quick view modal handling (if exists)
    const quickViewModal = document.getElementById('quickViewModal');
    if (quickViewModal) {
        quickViewModal.addEventListener('shown.bs.modal', function() {
            // Reinitialize any carousels inside the modal
            const modalCarousels = quickViewModal.querySelectorAll('.swiper-container');
            modalCarousels.forEach(carousel => {
                // Trigger resize if swiper is used
                window.dispatchEvent(new Event('resize'));
            });
        });
    }
});

// Global function for variant selection
window.selectVariant = function(card, variantId) {
    if (card.classList.contains('opacity-50')) return;

    document.querySelectorAll('.variant-card').forEach(c => c.classList.remove('border-primary', 'selected'));
    card.classList.add('border-primary', 'selected');

    // Update hidden input if exists
    const form = document.getElementById('addToCartForm');
    if (form) {
        let variantInput = form.querySelector('input[name="variantId"]');
        if (!variantInput) {
            variantInput = document.createElement('input');
            variantInput.type = 'hidden';
            variantInput.name = 'variantId';
            form.appendChild(variantInput);
        }
        variantInput.value = variantId;
    }

    const price = Number(card.dataset.price || 0);
    const priceEl = document.getElementById('productPrice');
    if (priceEl && price > 0) {
        priceEl.textContent = price.toLocaleString('vi-VN') + ' VND';
        priceEl.dataset.moneyVnd = String(price);
        document.dispatchEvent(new CustomEvent('furnish:money-updated'));
    }

    const imageUrl = card.dataset.image;
    const mainImage = document.getElementById('mainProductImage');
    if (imageUrl && mainImage) {
        mainImage.src = imageUrl;
    }
};

// Function to change main product image
window.changeMainImage = function(url, thumb) {
    const mainImage = document.getElementById('mainProductImage');
    if (mainImage) {
        mainImage.src = url;
        document.querySelectorAll('.thumbnail-img').forEach(t => {
            t.classList.remove('border-primary');
            t.classList.remove('border-2');
        });
        thumb.classList.add('border-primary', 'border-2');
    }
};

// Function to increment/decrement quantity
window.incrementQty = function() {
    const input = document.getElementById('quantity');
    if (input) {
        let value = parseInt(input.value) || 1;
        if (value < 99) input.value = value + 1;
    }
};

window.decrementQty = function() {
    const input = document.getElementById('quantity');
    if (input) {
        let value = parseInt(input.value) || 1;
        if (value > 1) input.value = value - 1;
    }
};
