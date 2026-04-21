document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach((element) => {
        new bootstrap.Tooltip(element);
    });

    const form = document.querySelector("[data-user-admin-form]");
    if (!form) {
        return;
    }

    const selectAll = form.querySelector("[data-select-all]");
    const filterInput = document.querySelector("[data-user-filter]");
    const actionButtons = [...form.querySelectorAll("[data-bulk-action-button]")];

    const getRows = () => [...form.querySelectorAll("[data-user-row]")];
    const getVisibleRows = () => getRows().filter((row) => row.style.display !== "none");
    const getVisibleCheckboxes = () =>
        getVisibleRows()
            .map((row) => row.querySelector("[data-select-row]"))
            .filter((checkbox) => checkbox);

    const updateSelectionState = () => {
        const visibleCheckboxes = getVisibleCheckboxes();
        const selectedCount = visibleCheckboxes.filter((checkbox) => checkbox.checked).length;

        actionButtons.forEach((button) => {
            button.disabled = selectedCount === 0;
        });

        if (!selectAll) {
            return;
        }

        if (visibleCheckboxes.length === 0) {
            selectAll.checked = false;
            selectAll.indeterminate = false;
            selectAll.disabled = true;
            return;
        }

        selectAll.disabled = false;
        selectAll.checked = selectedCount === visibleCheckboxes.length;
        selectAll.indeterminate = selectedCount > 0 && selectedCount < visibleCheckboxes.length;
    };

    const applyFilter = () => {
        const query = filterInput ? filterInput.value.trim().toLowerCase() : "";

        getRows().forEach((row) => {
            const checkbox = row.querySelector("[data-select-row]");
            const matches = row.innerText.toLowerCase().includes(query);

            row.style.display = matches ? "" : "none";
            if (!matches && checkbox) {
                checkbox.checked = false;
            }
        });

        updateSelectionState();
    };

    if (selectAll) {
        selectAll.addEventListener("change", () => {
            getVisibleCheckboxes().forEach((checkbox) => {
                checkbox.checked = selectAll.checked;
            });

            updateSelectionState();
        });
    }

    form.addEventListener("change", (event) => {
        if (event.target.matches("[data-select-row]")) {
            updateSelectionState();
        }
    });

    if (filterInput) {
        filterInput.addEventListener("input", applyFilter);
    }

    updateSelectionState();
});
