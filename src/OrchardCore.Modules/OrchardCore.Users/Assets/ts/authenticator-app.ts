const qrCodeData = document.getElementById("qrCodeData");
const qrCodeContainer = document.getElementById("qrCode");
const uri = qrCodeData?.dataset.url;

if (qrCodeContainer && uri) {
    new QRCode(qrCodeContainer, { text: uri, width: 150, height: 150 });
}
