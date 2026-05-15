(function () {
    "use strict";

    var acceptedFormats = ["image/jpeg", "image/jpg", "image/gif", "image/png", "image/bmp", "image/webp", "image/avif", "image/svg+xml"];

    function isValidImage(file) {
        if (!file) return false;
        if (acceptedFormats.indexOf(file.type) >= 0) return true;

        var name = (file.name || "").toLowerCase();
        var ext = name.split(".").pop();
        return ["jpg", "jpeg", "gif", "png", "bmp", "webp", "avif", "svg"].indexOf(ext) >= 0;
    }

    function applyImageData(controlId, previousUrl, dataUrl) {
        var input = document.getElementById(controlId);
        if (!input) return;
        input.value = (previousUrl || "") + "|" + dataUrl;
        input.setCustomValidity("");
    }

    function installImageUploader(container) {
        var controlId = container.getAttribute("controlId");
        if (!controlId) return;

        var imageSrc = container.getAttribute("imageSrc") || "";
        var title = container.getAttribute("title") || "Choisir une image";

        container.style.display = "flex";
        container.style.flexDirection = "column";
        container.style.alignItems = "center";

        var uploadedImage = document.createElement("img");
        uploadedImage.id = controlId + "_UploadedImage";
        uploadedImage.className = "UploadedImage";
        uploadedImage.src = imageSrc;
        uploadedImage.title = title;
        uploadedImage.tabIndex = 0;
        uploadedImage.style.width = "100%";

        var imageUploader = document.createElement("input");
        imageUploader.id = controlId + "_ImageUploader";
        imageUploader.type = "file";
        imageUploader.accept = acceptedFormats.join(",");
        imageUploader.style.visibility = "hidden";
        imageUploader.style.height = "0";
        imageUploader.style.width = "0";
        imageUploader.style.border = "0";
        imageUploader.style.margin = "0";

        var imageDataInput = document.createElement("input");
        imageDataInput.id = controlId;
        imageDataInput.name = controlId;
        imageDataInput.value = imageSrc;
        imageDataInput.style.visibility = "hidden";
        imageDataInput.style.height = "0";

        container.appendChild(uploadedImage);
        container.appendChild(imageUploader);
        container.appendChild(imageDataInput);

        function loadFile(file) {
            if (!isValidImage(file)) {
                alert("Ce format d'image n'est pas accepte.");
                return;
            }

            var reader = new FileReader();
            reader.onloadend = function () {
                uploadedImage.src = reader.result;
                applyImageData(controlId, imageSrc, reader.result);
            };
            reader.readAsDataURL(file);
        }

        uploadedImage.addEventListener("click", function () {
            imageUploader.click();
        });

        uploadedImage.addEventListener("keydown", function (event) {
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                imageUploader.click();
            }
        });

        uploadedImage.addEventListener("dragover", function (event) {
            event.preventDefault();
            uploadedImage.style.borderColor = "dodgerblue";
        });

        uploadedImage.addEventListener("dragleave", function (event) {
            event.preventDefault();
            uploadedImage.style.borderColor = "#d3d3d3";
        });

        uploadedImage.addEventListener("drop", function (event) {
            event.preventDefault();
            uploadedImage.style.borderColor = "#d3d3d3";
            var files = event.dataTransfer && event.dataTransfer.files;
            if (files && files.length > 0)
                loadFile(files[0]);
        });

        imageUploader.addEventListener("change", function () {
            if (imageUploader.files && imageUploader.files.length > 0)
                loadFile(imageUploader.files[0]);
        });
    }

    $(function () {
        $(".imageUploader").each(function () {
            installImageUploader(this);
        });
    });
})();
