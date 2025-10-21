
const dropArea = document.getElementById('drop-area');
const fileInput = document.getElementById('fileInput');
const form = document.getElementById('uploadForm');

dropArea.addEventListener('click', () => fileInput.click());

dropArea.addEventListener('dragover', (e) => {
    e.preventDefault();
    dropArea.classList.add('bg-primary', 'text-white');
});

dropArea.addEventListener('dragleave', () => {
    dropArea.classList.remove('bg-primary', 'text-white');
});

dropArea.addEventListener('drop', (e) => {
    e.preventDefault();
    dropArea.classList.remove('bg-primary', 'text-white');
    const file = e.dataTransfer.files[0];
    uploadFile(file);
});

fileInput.addEventListener('change', (e) => {
    const file = e.target.files[0];
    uploadFile(file);
});

function uploadFile(file) {
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    fetch('/LogFiles/Upload', {
        method: 'POST',
        body: formData
    }).then(response => {
        if (response.redirected) {
            window.location.href = response.url;
        } else {
            alert('Upload failed');
        }
    });
}