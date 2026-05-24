(function () {
    const storageKey = "furnish.displayCurrency.v1";
    const supportedCurrencies = ["VND", "USD"];
    const state = {
        currency: supportedCurrencies.includes(localStorage.getItem(storageKey)) ? localStorage.getItem(storageKey) : "VND",
        rate: null,
        rateCurrency: null,
        loading: null
    };

    function parseAmount(value) {
        const amount = Number(value);
        return Number.isFinite(amount) && amount >= 0 ? amount : null;
    }

    function formatForeign(amount, currency) {
        return new Intl.NumberFormat(currency === "USD" ? "en-US" : "vi-VN", {
            style: "currency",
            currency,
            maximumFractionDigits: amount >= 100 ? 0 : 2
        }).format(amount);
    }

    function getEstimateElement(el) {
        const next = el.nextElementSibling;
        if (next && next.classList.contains("money-estimate")) {
            return next;
        }

        const estimate = document.createElement("small");
        estimate.className = "money-estimate";
        estimate.hidden = true;
        el.insertAdjacentElement("afterend", estimate);
        return estimate;
    }

    function updateControls() {
        document.querySelectorAll("[data-currency-choice]").forEach(btn => {
            const active = btn.dataset.currencyChoice === state.currency;
            btn.classList.toggle("active", active);
            btn.classList.toggle("is-active", active);
        });

        document.querySelectorAll("[data-currency-label]").forEach(label => {
            label.textContent = state.currency;
        });
    }

    async function loadRate(currency) {
        if (currency === "VND") return null;
        if (state.rate && state.rateCurrency === currency) return state.rate;
        if (state.loading) return state.loading;

        state.loading = fetch(`/Currency/Rate?target=${encodeURIComponent(currency)}`, {
            headers: { "Accept": "application/json" }
        })
            .then(response => {
                if (!response.ok) throw new Error("Could not load exchange rate.");
                return response.json();
            })
            .then(data => {
                const rate = Number(data.rate);
                if (!Number.isFinite(rate) || rate <= 0) throw new Error("Invalid exchange rate.");
                state.rate = { rate, source: data.source, updatedAtUtc: data.updatedAtUtc, isFallback: Boolean(data.isFallback) };
                state.rateCurrency = currency;
                return state.rate;
            })
            .finally(() => {
                state.loading = null;
            });

        return state.loading;
    }

    async function renderMoney() {
        updateControls();
        const moneyEls = Array.from(document.querySelectorAll("[data-money-vnd]"));

        if (state.currency === "VND") {
            moneyEls.forEach(el => {
                const estimate = getEstimateElement(el);
                estimate.hidden = true;
                estimate.textContent = "";
            });
            return;
        }

        let quote = null;
        try {
            quote = await loadRate(state.currency);
        } catch {
            moneyEls.forEach(el => {
                const estimate = getEstimateElement(el);
                estimate.hidden = true;
                estimate.textContent = "";
            });
            return;
        }

        moneyEls.forEach(el => {
            const amount = parseAmount(el.dataset.moneyVnd);
            const estimate = getEstimateElement(el);
            if (amount === null || !quote) {
                estimate.hidden = true;
                estimate.textContent = "";
                return;
            }

            estimate.textContent = `~ ${formatForeign(amount * quote.rate, state.currency)}`;
            estimate.title = quote.isFallback
                ? "Tỉ giá dự phòng, chỉ để tham khảo"
                : `Tỉ giá tham khảo từ ${quote.source}`;
            estimate.hidden = false;
        });
    }

    function setCurrency(currency) {
        if (!supportedCurrencies.includes(currency)) return;
        state.currency = currency;
        localStorage.setItem(storageKey, currency);
        renderMoney();
    }

    let mutationTimer = null;
    function scheduleRender() {
        window.clearTimeout(mutationTimer);
        mutationTimer = window.setTimeout(renderMoney, 80);
    }

    document.addEventListener("DOMContentLoaded", () => {
        document.querySelectorAll("[data-currency-choice]").forEach(btn => {
            btn.addEventListener("click", () => setCurrency(btn.dataset.currencyChoice));
        });

        renderMoney();

        const observer = new MutationObserver(mutations => {
            if (mutations.some(mutation => Array.from(mutation.addedNodes).some(node =>
                node.nodeType === 1
                && !node.classList?.contains("money-estimate")
                && (node.matches?.("[data-money-vnd]") || node.querySelector?.("[data-money-vnd]"))))) {
                scheduleRender();
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });
    });

    document.addEventListener("furnish:money-updated", renderMoney);

    window.FurnishCurrency = {
        refresh: renderMoney,
        setCurrency
    };
})();
