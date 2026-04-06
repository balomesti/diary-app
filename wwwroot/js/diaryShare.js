window.DiaryShare = {

    captureAndDownload: async function (elementId, filename) {
        const el = document.getElementById(elementId);
        if (!el) { console.warn('DiaryShare: element not found:', elementId); return; }

        const prev = {
            position: el.style.position,
            top:      el.style.top,
            left:     el.style.left,
        };
        el.style.position = 'fixed';
        el.style.top      = '0';
        el.style.left     = '0';

        try {
            const canvas = await html2canvas(el, {
                scale:           2,
                useCORS:         true,
                backgroundColor: null,
                logging:         false,
            });

            el.style.position = prev.position;
            el.style.top      = prev.top;
            el.style.left     = prev.left;

            const dataUrl = canvas.toDataURL('image/png');

            if (navigator.share && navigator.canShare) {
                const blob = await (await fetch(dataUrl)).blob();
                const file = new File([blob], filename, { type: 'image/png' });
                if (navigator.canShare({ files: [file] })) {
                    await navigator.share({ files: [file], title: 'Diary Me' });
                    return;
                }
            }

            const link      = document.createElement('a');
            link.href       = dataUrl;
            link.download   = filename;
            link.click();

        } catch (err) {
            el.style.position = prev.position;
            el.style.top      = prev.top;
            el.style.left     = prev.left;
            console.error('DiaryShare: capture failed', err);
        }
    },

    downloadText: function (text, filename) {
        const blob = new Blob([text], { type: 'text/plain' });
        const url  = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href     = url;
        link.download = filename;
        link.click();
        URL.revokeObjectURL(url);
    }
};

window.downloadEntryAsImage = async function (elementId, fileName) {
    const el = document.getElementById(elementId);
    if (!el) return;

    const canvas = await html2canvas(el, {
        useCORS: true,
        allowTaint: true,
        backgroundColor: null, // use CSS background (gradient)
        scale: 3, // higher scale for clarity
        logging: false,
    });

    const { jsPDF } = window.jspdf;
    
    // Calculate PDF dimensions based on element ratio
    const imgWidth = 210; // A4 width in mm
    const imgHeight = (canvas.height * imgWidth) / canvas.width;
    
    const pdf = new jsPDF({
        orientation: imgWidth > imgHeight ? 'landscape' : 'portrait',
        unit: 'mm',
        format: [imgWidth, imgHeight] // Custom format to fit the card perfectly
    });

    pdf.addImage(canvas.toDataURL('image/png', 1.0), 'PNG', 0, 0, imgWidth, imgHeight, undefined, 'FAST');
    pdf.save(fileName.replace('.png', '.pdf'));
};