// =============================================================
// TP2 Shop — Client-Side Interactivity
// - Sidebar toggle (Desktop persistence + Mobile offcanvas)
// - Live AJAX Add-to-cart & Cart Badge update
// - Dynamic Toast notifications
// - Checkout form double-submit prevention
// =============================================================

document.addEventListener("DOMContentLoaded", function () {

    // ── 1. Toast Notification Helper ─────────────────────────────
    function showToast(message, type) {
        type = type || "success";
        const container = document.getElementById("toastContainer");
        if (!container) return;

        const toast = document.createElement("div");
        toast.className = "custom-toast" + (type === "error" ? " toast-error" : "");
        toast.setAttribute("role", "alert");

        const iconClass = type === "error" ? "bi-exclamation-triangle-fill text-danger" : "bi-check-circle-fill text-success";
        toast.innerHTML = `
            <i class="bi ${iconClass} fs-5"></i>
            <div class="flex-grow-1 font-weight-500">${message}</div>
            <button type="button" class="btn-close btn-sm" aria-label="Fermer"></button>
        `;

        toast.querySelector(".btn-close").addEventListener("click", function () {
            toast.remove();
        });

        container.appendChild(toast);

        setTimeout(function () {
            toast.style.transition = "opacity 0.5s ease, transform 0.5s ease";
            toast.style.opacity = "0";
            toast.style.transform = "translateX(50px)";
            setTimeout(function () { toast.remove(); }, 500);
        }, 4000);
    }

    // Auto-dismiss existing server toasts after 4 seconds
    document.querySelectorAll(".custom-toast").forEach(function (toast) {
        setTimeout(function () {
            toast.style.transition = "opacity 0.5s ease, transform 0.5s ease";
            toast.style.opacity = "0";
            toast.style.transform = "translateX(50px)";
            setTimeout(function () { toast.remove(); }, 500);
        }, 4000);
    });

    // ── 2. Sidebar Toggle & Persistence ──────────────────────────
    const btnCategoriesToggle = document.getElementById("btnCategoriesToggle");
    const desktopSidebarCol = document.getElementById("desktopSidebarCol");
    const desktopContentCol = document.getElementById("desktopContentCol");
    const offcanvasEl = document.getElementById("offcanvasCategories");

    function isProductListPage() {
        const path = window.location.pathname.toLowerCase();
        return path === "/product" || path === "/product/index" || path.startsWith("/product?");
    }

    if (desktopSidebarCol && desktopContentCol) {
        // Read persisted state
        const storedPref = localStorage.getItem("tp2_sidebar_open");
        let shouldBeOpen = true;

        if (storedPref !== null) {
            shouldBeOpen = (storedPref === "true");
        } else {
            // Default open on product list page, closed on other pages
            shouldBeOpen = isProductListPage();
        }

        applySidebarState(shouldBeOpen);

        if (btnCategoriesToggle) {
            btnCategoriesToggle.addEventListener("click", function () {
                if (window.innerWidth < 992 && offcanvasEl && window.bootstrap) {
                    const bsOffcanvas = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
                    bsOffcanvas.toggle();
                } else {
                    const isCurrentlyOpen = !desktopSidebarCol.classList.contains("d-lg-none");
                    const newState = !isCurrentlyOpen;
                    applySidebarState(newState);
                    localStorage.setItem("tp2_sidebar_open", newState);
                }
            });
        }
    } else if (btnCategoriesToggle && offcanvasEl && window.bootstrap) {
        btnCategoriesToggle.addEventListener("click", function () {
            const bsOffcanvas = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
            bsOffcanvas.toggle();
        });
    }

    function applySidebarState(open) {
        if (!desktopSidebarCol || !desktopContentCol) return;
        if (open) {
            desktopSidebarCol.classList.remove("d-lg-none");
            desktopContentCol.classList.remove("col-lg-12");
            desktopContentCol.classList.add("col-lg-9", "col-xl-10");
        } else {
            desktopSidebarCol.classList.add("d-lg-none");
            desktopContentCol.classList.remove("col-lg-9", "col-xl-10");
            desktopContentCol.classList.add("col-lg-12");
        }
    }

    // ── 3. Live Cart Badge & AJAX Add-to-Cart ────────────────────
    function updateCartBadge(count) {
        const badge = document.getElementById("cartBadge");
        if (!badge) return;

        badge.textContent = count;
        if (count > 0) {
            badge.style.display = "";
            badge.classList.add("animate__pulse");
            setTimeout(function () { badge.classList.remove("animate__pulse"); }, 600);
        } else {
            badge.style.display = "none";
        }
    }

    // Intercept Add to Cart forms via AJAX
    document.addEventListener("submit", function (e) {
        const form = e.target;
        if (form && form.classList.contains("add-to-cart-form")) {
            e.preventDefault();

            const submitBtn = form.querySelector('button[type="submit"]');
            const originalHtml = submitBtn ? submitBtn.innerHTML : "";
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Ajout...';
            }

            const formData = new FormData(form);

            fetch(form.action, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            })
            .then(function (res) {
                if (res.ok) return res.json();
                throw new Error("Erreur réseau");
            })
            .then(function (data) {
                if (data.cartCount !== undefined) {
                    updateCartBadge(data.cartCount);
                }
                showToast(data.message || "Produit ajouté au panier !", "success");
            })
            .catch(function (err) {
                showToast("Impossible d'ajouter le produit au panier.", "error");
            })
            .finally(function () {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalHtml;
                }
            });
        }
    });

    // ── 4. Checkout Double Submit Prevention ─────────────────────
    const checkoutForm = document.getElementById("checkoutForm");
    if (checkoutForm) {
        checkoutForm.addEventListener("submit", function () {
            const submitBtn = checkoutForm.querySelector("#btnSubmitOrder");
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> Traitement de votre commande...';
            }
        });
    }

    // ── 5. Payment Radio Cards Selection ─────────────────────────
    const paymentCards = document.querySelectorAll(".payment-radio-card");
    paymentCards.forEach(function (card) {
        card.addEventListener("click", function () {
            paymentCards.forEach(c => c.classList.remove("selected"));
            card.classList.add("selected");
            const radio = card.querySelector('input[type="radio"]');
            if (radio) radio.checked = true;
        });
    });

});
