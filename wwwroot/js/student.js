(function () {
    'use strict';

    const page = document.getElementById('studentPage');
    if (!page) return;

    const urls = {
        list: page.dataset.listUrl,
        get: page.dataset.getUrl,
        create: page.dataset.createUrl,
        edit: page.dataset.editUrl,
        remove: page.dataset.deleteUrl
    };

    const ajaxHeaders = { 'X-Requested-With': 'XMLHttpRequest' };
    const modalOptions = { backdrop: 'static', keyboard: false };

    const studentModal = new bootstrap.Modal(document.getElementById('studentModal'), modalOptions);
    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'), modalOptions);

    const studentForm = document.getElementById('studentForm');
    const deleteForm = document.getElementById('deleteForm');
    const studentTitle = document.getElementById('studentModalTitle');
    const saveButton = document.getElementById('btnSaveStudent');
    const deleteButton = document.getElementById('btnConfirmDelete');
    const tableHost = document.getElementById('studentTable');
    const alertHost = document.getElementById('alertHost');

    function showAlert(message, type) {
        alertHost.innerHTML = '';

        const div = document.createElement('div');
        div.className = 'alert alert-' + type + ' alert-dismissible';
        div.setAttribute('role', 'alert');
        div.textContent = message;

        const close = document.createElement('button');
        close.type = 'button';
        close.className = 'btn-close';
        close.setAttribute('data-bs-dismiss', 'alert');
        close.setAttribute('aria-label', 'Close');
        div.appendChild(close);

        alertHost.appendChild(div);
    }

    function clearErrors(form) {
        form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
        form.querySelectorAll('[data-error-for]').forEach(el => { el.textContent = ''; });

        const summary = form.querySelector('.js-form-error');
        summary.textContent = '';
        summary.classList.add('d-none');
    }

    function showErrors(form, data) {
        const summary = form.querySelector('.js-form-error');
        const general = [];
        let fieldErrors = 0;

        Object.entries(data.errors || {}).forEach(([field, messages]) => {
            const input = form.elements[field];
            const holder = form.querySelector('[data-error-for="' + field + '"]');

            if (input && holder) {
                input.classList.add('is-invalid');
                holder.textContent = messages.join(' ');
                fieldErrors++;
            } else {
                general.push(...messages);
            }
        });

        if (!data.errors && data.message) general.push(data.message);
        if (!general.length && !fieldErrors) general.push('Something went wrong. Please try again.');

        if (general.length) {
            summary.textContent = general.join(' ');
            summary.classList.remove('d-none');
        }
    }

    async function refreshList() {
        const response = await fetch(urls.list + window.location.search, { headers: ajaxHeaders });
        if (!response.ok) throw new Error('Unable to refresh the student list.');
        tableHost.innerHTML = await response.text();
    }

    async function afterSuccess(message) {
    showAlert(message, 'success');

    setTimeout(() => {
        window.location.reload();
    }, 700);
    }

    async function fetchStudent(id) {
        const response = await fetch(urls.get + '?id=' + encodeURIComponent(id), { headers: ajaxHeaders });

        if (response.status === 403) throw new Error('You do not have permission to access this student record.');
        if (response.status === 404) throw new Error('This student record no longer exists.');
        if (!response.ok) throw new Error('Unable to load student details.');

        return await response.json();
    }

    async function postForm(form, url, button) {
        clearErrors(form);
        button.disabled = true;

        try {
            const response = await fetch(url, {
                method: 'POST',
                body: new FormData(form),
                headers: ajaxHeaders
            });

            let data = {};
            try { data = await response.json(); } catch { /* non-JSON response */ }

            return { ok: response.ok && data.success === true, data };
        } catch {
            return { ok: false, data: { message: 'Unable to reach the server. Please try again.' } };
        } finally {
            button.disabled = false;
        }
    }

    function setDepartment(value) {
        const field = studentForm.elements['Department'];

        // Keep the current value selectable even if the department is no longer active
        if (field.tagName === 'SELECT' && value &&
            !Array.from(field.options).some(o => o.value === value)) {
            field.add(new Option(value, value));
        }

        field.value = value;
    }

    // ---------- Add ----------

    document.getElementById('btnAddStudent').addEventListener('click', () => {
        clearErrors(studentForm);
        studentForm.reset();
        studentForm.elements['StudentID'].value = '0';
        studentTitle.textContent = 'Add Student';
        saveButton.textContent = 'Add Student';
        studentModal.show();
    });

    // ---------- Edit / Delete buttons (delegated, because the table is re-rendered) ----------

    tableHost.addEventListener('click', async (e) => {
        const editBtn = e.target.closest('.js-edit');
        const deleteBtn = e.target.closest('.js-delete');

        try {
            if (editBtn) {
                const s = await fetchStudent(editBtn.dataset.id);

                clearErrors(studentForm);
                studentForm.reset();
                studentForm.elements['StudentID'].value = s.studentID;
                studentForm.elements['Name'].value = s.name;
                studentForm.elements['RollNo'].value = s.rollNo;
                studentForm.elements['Marks'].value = s.marks;
                setDepartment(s.department);

                studentTitle.textContent = 'Edit Student';
                saveButton.textContent = 'Save Changes';
                studentModal.show();
            } else if (deleteBtn) {
                const s = await fetchStudent(deleteBtn.dataset.id);

                clearErrors(deleteForm);
                deleteForm.elements['id'].value = s.studentID;
                deleteForm.querySelectorAll('[data-field]').forEach(el => {
                    el.textContent = s[el.dataset.field];
                });
                deleteModal.show();
            }
        } catch (err) {
            showAlert(err.message, 'danger');
        }
    });

    // ---------- Submit handlers ----------

    studentForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const isEdit = Number(studentForm.elements['StudentID'].value) > 0;
        const result = await postForm(studentForm, isEdit ? urls.edit : urls.create, saveButton);

        if (result.ok) {
            studentModal.hide();
            await afterSuccess(result.data.message);
        } else {
            showErrors(studentForm, result.data);
        }
    });

    deleteForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const result = await postForm(deleteForm, urls.remove, deleteButton);

        if (result.ok) {
            deleteModal.hide();
            await afterSuccess(result.data.message);
        } else {
            showErrors(deleteForm, result.data);
        }
    });
})();