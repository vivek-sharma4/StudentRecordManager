function getAntiForgeryToken() {
    return document.querySelector(
        'input[name="__RequestVerificationToken"]'
    )?.value || '';
}

function showPageMessage(message, type = 'success') {
    const banner = document.getElementById('pageAlertBanner');
    const text = document.getElementById('pageAlertText');

    if (!banner || !text) {
        alert(message);
        return;
    }

    banner.className = `alert alert-${type} alert-dismissible fade show`;
    text.textContent = message;
    banner.style.display = 'block';
}

function showModalError(elementId, message) {
    const element = document.getElementById(elementId);

    if (element) {
        element.textContent = message;
        element.style.display = 'block';
    }
}

function clearModalError(elementId) {
    const element = document.getElementById(elementId);

    if (element) {
        element.textContent = '';
        element.style.display = 'none';
    }
}

async function getErrorMessage(response) {
    try {
        const data = await response.json();
        return data.message || data.title || 'Something went wrong.';
    } catch {
        return 'Request failed. Please try again.';
    }
}

function closeModal(modalId) {
    const modalElement = document.getElementById(modalId);

    if (modalElement) {
        bootstrap.Modal.getOrCreateInstance(modalElement).hide();
    }
}

async function openEditModal(id) {
    try {
        const response = await fetch(`/Student/GetStudent/${id}`);

        if (!response.ok) {
            throw new Error(await getErrorMessage(response));
        }

        const student = await response.json();

        document.getElementById('editStudentID').value = student.studentID;
        document.getElementById('editStudentName').value = student.name;
        document.getElementById('editStudentRollNo').value = student.rollNo;
        document.getElementById('editStudentDept').value = student.department;
        document.getElementById('editStudentMarks').value = student.marks;

        clearModalError('editModalError');

        bootstrap.Modal.getOrCreateInstance(
            document.getElementById('editStudentModal')
        ).show();
    }
    catch (error) {
        showPageMessage(error.message, 'danger');
    }
}

async function submitEditStudent(event) {
    event.preventDefault();

    clearModalError('editModalError');

    const student = {
        StudentID: parseInt(document.getElementById('editStudentID').value),
        Name: document.getElementById('editStudentName').value.trim(),
        RollNo: document.getElementById('editStudentRollNo').value.trim(),
        Department: document.getElementById('editStudentDept').value.trim(),
        Marks: parseFloat(document.getElementById('editStudentMarks').value)
    };

    try {
        const response = await fetch('/Student/EditModal', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify(student)
        });

        const data = await response.json();

        if (!response.ok || !data.success) {
            throw new Error(data.message || 'Unable to update student.');
        }

        closeModal('editStudentModal');
        showPageMessage(data.message || 'Student updated successfully!');

        setTimeout(() => window.location.reload(), 800);
    }
    catch (error) {
        showModalError('editModalError', error.message);
    }
}

let selectedStudentId = null;
function openDeleteModal(id, name, rollNo, department) {
    selectedStudentId = id;

    document.getElementById('delModalName').textContent = name;
    document.getElementById('delModalRollNo').textContent = rollNo;
    document.getElementById('delModalDept').textContent = department;

    bootstrap.Modal.getOrCreateInstance(
        document.getElementById('deleteStudentModal')
    ).show();
}

async function executeDeleteStudent() {
    if (!selectedStudentId) {
        showPageMessage('Please select a student to delete.', 'danger');
        return;
    }

    try {
        const response = await fetch(
            `/Student/DeleteModal?id=${selectedStudentId}`,
            {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            }
        );

        const data = await response.json();

        if (!response.ok || !data.success) {
            throw new Error(data.message || 'Unable to delete student.');
        }

        closeModal('deleteStudentModal');
        showPageMessage(data.message || 'Student deleted successfully!');

        setTimeout(() => window.location.reload(), 800);
    }
    catch (error) {
        showPageMessage(error.message, 'danger');
    }
}

async function submitCreateStudent(event) {
    event.preventDefault();

    clearModalError('createModalError');

    const form = document.getElementById('createStudentForm');

    const student = {
        Name: form.querySelector('[name="Name"]').value.trim(),
        RollNo: form.querySelector('[name="RollNo"]').value.trim(),
        Department: form.querySelector('[name="Department"]').value.trim(),
        Marks: parseFloat(form.querySelector('[name="Marks"]').value)
    };

    try {
        const response = await fetch('/Student/CreateModal', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify(student)
        });

        const data = await response.json();

        if (!response.ok || !data.success) {
            throw new Error(data.message || 'Unable to create student.');
        }

        closeModal('createStudentModal');
        showPageMessage(data.message || 'Student created successfully!');

        setTimeout(() => window.location.reload(), 800);
    }
    catch (error) {
        showModalError('createModalError', error.message);
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const createForm = document.getElementById('createStudentForm');

    if (createForm) {
        createForm.addEventListener('submit', submitCreateStudent);
    }
});
