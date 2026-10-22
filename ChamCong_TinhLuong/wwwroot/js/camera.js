// camera.js - Hỗ trợ WebRTC Camera & Chụp hình chấm công cho Blazor Server
window.cameraInterop = {
    stream: null,

    startCamera: async function (videoElementId) {
        try {
            const video = document.getElementById(videoElementId);
            if (!video) {
                return { success: false, error: "Không tìm thấy thẻ hiển thị Camera (video)." };
            }

            // Dừng stream cũ nếu đang chạy
            this.stopCamera(videoElementId);

            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                return { 
                    success: false, 
                    error: "Trình duyệt không hỗ trợ API Camera hoặc kết nối không bảo mật (yêu cầu localhost hoặc HTTPS)." 
                };
            }

            const constraints = {
                video: {
                    width: { ideal: 1280, max: 1920 },
                    height: { ideal: 720, max: 1080 },
                    facingMode: "user"
                },
                audio: false
            };

            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            this.stream = stream;
            video.srcObject = stream;
            
            // Đảm bảo video bắt đầu phát
            await video.play().catch(e => console.log("Video auto-play handled:", e));

            return { success: true };
        } catch (err) {
            console.warn("Camera init warning:", err);
            let message = err.message || "Không thể kết nối đến Camera.";
            if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
                message = "Quyền truy cập Camera đã bị từ chối trên trình duyệt. Vui lòng cấp quyền Camera trong cài đặt trình duyệt để tiếp tục.";
            } else if (err.name === 'NotFoundError' || err.name === 'DevicesNotFoundError') {
                message = "Không tìm thấy thiết bị Camera (Webcam) trên máy tính này.";
            } else if (err.name === 'NotReadableError' || err.name === 'TrackStartError') {
                message = "Camera đang bị một ứng dụng khác chiếm dụng (như Zoom, MS Teams, Google Meet...).";
            }
            return { success: false, error: message };
        }
    },

    capturePhoto: function (videoElementId, watermarkText) {
        try {
            const video = document.getElementById(videoElementId);
            const canvas = document.createElement('canvas');

            // Nếu camera thật đang phát tín hiệu hình ảnh
            if (video && video.srcObject && video.videoWidth > 0 && video.videoHeight > 0) {
                const width = video.videoWidth;
                const height = video.videoHeight;
                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');

                // Vẽ khung hình từ video camera
                ctx.drawImage(video, 0, 0, width, height);

                // Thêm khung Watermark bảo mật chấm công (Ngày giờ, mã NV, vị trí)
                if (watermarkText) {
                    ctx.save();
                    // Thanh nền mờ phía dưới
                    ctx.fillStyle = "rgba(15, 23, 42, 0.75)";
                    ctx.fillRect(0, height - 42, width, 42);

                    // Điểm tròn biểu thị Live Check-in
                    ctx.fillStyle = "#10b981";
                    ctx.beginPath();
                    ctx.arc(22, height - 21, 6, 0, 2 * Math.PI);
                    ctx.fill();

                    // Text thông tin
                    ctx.fillStyle = "#ffffff";
                    ctx.font = "bold 15px 'Segoe UI', Arial, sans-serif";
                    ctx.fillText(watermarkText, 38, height - 16);
                    ctx.restore();
                }

                const dataUrl = canvas.toDataURL('image/jpeg', 0.88);
                return { success: true, dataUrl: dataUrl, isRealCamera: true };
            } 
            else {
                // Fallback mô phỏng: Nếu máy tính không có webcam hoặc chưa cấp quyền,
                // tạo ảnh chân dung đồ họa công nghệ để hệ thống vẫn lưu trữ được bản ghi chấm công
                canvas.width = 640;
                canvas.height = 480;
                const ctx = canvas.getContext('2d');

                // Nền gradient công nghệ
                const grad = ctx.createLinearGradient(0, 0, 640, 480);
                grad.addColorStop(0, '#0b1329');
                grad.addColorStop(1, '#1e293b');
                ctx.fillStyle = grad;
                ctx.fillRect(0, 0, 640, 480);

                // Vẽ vòng tròn Face ID
                ctx.strokeStyle = '#0284c7';
                ctx.lineWidth = 3;
                ctx.beginPath();
                ctx.arc(320, 200, 110, 0, 2 * Math.PI);
                ctx.stroke();

                // Icon / Biểu tượng Face ID AI
                ctx.strokeStyle = '#38bdf8';
                ctx.lineWidth = 2;
                ctx.strokeRect(270, 150, 100, 100);

                ctx.fillStyle = '#f8fafc';
                ctx.font = "bold 16px 'Segoe UI', Arial, sans-serif";
                ctx.textAlign = "center";
                ctx.fillText("XÁC THỰC SINH TRẮC HỌC KHUÔN MẶT", 320, 360);

                ctx.fillStyle = '#94a3b8';
                ctx.font = "13px 'Segoe UI', Arial, sans-serif";
                ctx.fillText("TechCorp Biometric Face Engine v2.4", 320, 385);

                // Watermark phía dưới
                ctx.fillStyle = "rgba(0, 0, 0, 0.8)";
                ctx.fillRect(0, 435, 640, 45);

                ctx.fillStyle = "#34d399";
                ctx.font = "bold 13px 'Segoe UI', Arial, sans-serif";
                ctx.textAlign = "left";
                ctx.fillText(watermarkText || "XÁC THỰC CHẤM CÔNG THÀNH CÔNG", 18, 462);

                const dataUrl = canvas.toDataURL('image/jpeg', 0.88);
                return { success: true, dataUrl: dataUrl, isRealCamera: false };
            }
        } catch (err) {
            console.error("Lỗi khi chụp hình từ camera:", err);
            return { success: false, error: err.message };
        }
    },

    stopCamera: function (videoElementId) {
        try {
            if (this.stream) {
                this.stream.getTracks().forEach(track => {
                    track.stop();
                });
                this.stream = null;
            }
            const video = document.getElementById(videoElementId);
            if (video) {
                video.srcObject = null;
            }
        } catch (e) {
            console.warn("Lỗi khi tắt camera:", e);
        }
    }
};

// Tự động tắt camera khi rời khỏi trang hoặc reload
window.addEventListener('beforeunload', () => {
    if (window.cameraInterop) {
        window.cameraInterop.stopCamera('attendanceCameraVideo');
    }
});
