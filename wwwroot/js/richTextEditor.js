window.RichTextEditor = (() => {
    const instances = {};

    return {
        init(editorId, editorRef, initialValue, placeholder, minHeight, maxChars) {
            const shell = document.getElementById(`editorShell-${editorId}`);
            const editorEl = document.getElementById(`editorContent-${editorId}`);
            if (!editorEl) return;

            editorEl.style.minHeight = `${minHeight}px`;

            if (initialValue) {
                editorEl.innerHTML = initialValue;
            }

            editorEl.addEventListener('input', () => {
                const html = editorEl.innerHTML;
                const text = editorEl.innerText;
                const content = text.trim() === '' ? '' : html;
                const event = new CustomEvent('rte-content-change', {
                    detail: { html: content, text: text }
                });
                editorEl.dispatchEvent(event);
            });

            editorEl.addEventListener('paste', e => {
                e.preventDefault();
                const text = (e.clipboardData || window.clipboardData).getData('text/plain');
                document.execCommand('insertText', false, text);
            });

            instances[editorId] = {
                editor: editorEl,
                shell: shell,
                aiLoading: false
            };
        },

        destroy(editorId) {
            delete instances[editorId];
        },

        getContent(editorId) {
            const inst = instances[editorId];
            if (!inst) return '';
            const text = inst.editor.innerText.trim();
            return text === '' ? '' : inst.editor.innerHTML;
        },

        execCommand(editorId, command) {
            const inst = instances[editorId];
            if (!inst) return;
            inst.editor.focus();
            document.execCommand(command, false, null);
        },

        toggleBlockquote(editorId) {
            const inst = instances[editorId];
            if (!inst) return;
            const editor = inst.editor;
            editor.focus();
            const sel = window.getSelection();
            if (!sel.rangeCount) return;
            const node = sel.getRangeAt(0).commonAncestorContainer;
            const el = node.nodeType === 1 ? node : node.parentElement;
            const bq = el.closest('blockquote');
            if (bq) {
                const p = document.createElement('p');
                p.innerHTML = bq.innerHTML;
                bq.parentNode.replaceChild(p, bq);
            } else {
                document.execCommand('formatBlock', false, 'blockquote');
            }
        },

        applyHeading(editorId, tag) {
            const inst = instances[editorId];
            if (!inst) return;
            inst.editor.focus();
            document.execCommand('formatBlock', false, tag || 'p');
        },

        clearFormat(editorId) {
            const inst = instances[editorId];
            if (!inst) return;
            inst.editor.focus();
            document.execCommand('removeFormat', false, null);
            document.execCommand('formatBlock', false, 'p');
        },

        queryCommandState(editorId, command) {
            try {
                return document.queryCommandState(command);
            } catch {
                return false;
            }
        },

        isInBlockquote(editorId) {
            const inst = instances[editorId];
            if (!inst) return false;
            const sel = window.getSelection();
            if (!sel || !sel.rangeCount) return false;
            const node = sel.getRangeAt(0).commonAncestorContainer;
            const el = node.nodeType === 1 ? node : node.parentElement;
            return !!el.closest('blockquote');
        },

        getCurrentHeading(editorId) {
            const inst = instances[editorId];
            if (!inst) return '';
            const sel = window.getSelection();
            if (!sel || !sel.rangeCount) return '';
            const node = sel.getRangeAt(0).commonAncestorContainer;
            const el = node.nodeType === 1 ? node : node.parentElement;
            const heading = el.closest('h1,h2,h3');
            return heading ? heading.tagName : '';
        },

        insertText(editorId, text) {
            const inst = instances[editorId];
            if (!inst) return;
            inst.editor.focus();
            const sel = window.getSelection();
            const range = sel.getRangeAt(0);
            range.selectNodeContents(inst.editor);
            range.collapse(false);
            sel.removeAllRanges();
            sel.addRange(range);
            document.execCommand('insertText', false, text);
        },

        setAiLoading(editorId, loading) {
            const inst = instances[editorId];
            if (!inst) return;
            inst.aiLoading = loading;
            const btn = inst.shell?.querySelector('.tb-ai');
            if (!btn) return;
            if (loading) {
                btn.innerHTML = '<span style="display:inline-block;animation:spin 0.8s linear infinite">⟳</span> Thinking...';
                btn.style.opacity = '0.7';
            } else {
                btn.innerHTML = '<span>✦</span> Inspire Me';
                btn.style.opacity = '';
            }
        },

        startVoice(editorId, dotnetRef) {
            try {
                if (!instances[editorId]) {
                    instances[editorId] = {};
                }

                navigator.mediaDevices.getUserMedia({ audio: true })
                    .then((stream) => {
                        const mediaRecorder = new MediaRecorder(stream, {
                            mimeType: 'audio/webm;codecs=opus'
                        });
                        const audioChunks = [];

                        mediaRecorder.ondataavailable = (event) => {
                            if (event.data.size > 0) {
                                audioChunks.push(event.data);
                            }
                        };

                        mediaRecorder.onstop = async () => {
                            stream.getTracks().forEach(track => track.stop());
                            const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });
                            const arrayBuffer = await audioBlob.arrayBuffer();
                            const base64Audio = btoa(String.fromCharCode.apply(null, new Uint8Array(arrayBuffer)));

                            try {
                                const transcript = await dotnetRef.invokeMethodAsync('TranscribeAudio', base64Audio);
                                if (transcript && transcript.trim()) {
                                    await dotnetRef.invokeMethodAsync('OnVoiceResult', ' ' + transcript.trim());
                                }
                            } catch (e) {
                                console.error('Transcription error:', e);
                            }

                            await dotnetRef.invokeMethodAsync('OnVoiceEnd');
                        };

                        mediaRecorder.onerror = (event) => {
                            console.error('MediaRecorder error:', event.error);
                            stream.getTracks().forEach(track => track.stop());
                            dotnetRef.invokeMethodAsync('OnVoiceEnd');
                        };

                        instances[editorId]._mediaRecorder = mediaRecorder;
                        instances[editorId]._recording = true;
                        mediaRecorder.start();
                    })
                    .catch((err) => {
                        console.error('Microphone permission denied:', err);
                        dotnetRef.invokeMethodAsync('OnVoiceEnd');
                    });

                return true;
            } catch (e) {
                console.error('startVoice error:', e);
                return false;
            }
        },

        stopVoice(editorId) {
            const inst = instances[editorId];
            if (inst && inst._mediaRecorder && inst._recording) {
                inst._mediaRecorder.stop();
                inst._recording = false;
            }
        }
    };
})();