import useResizeObserver from "@react-hook/resize-observer";
import { decode } from "base64-arraybuffer";
import React, { useRef, useEffect, useState, useMemo } from "react";

import Box from "@mui/material/Box";

import useMounted from "../../hooks/useMounted.js";
import { Routes } from "../../routes";
import useSignalR from "./useSignalR";
import placeholderImageUrl from "/public/640x480.jpg";

const placeholder = new Image();
placeholder.src = placeholderImageUrl;

const DetectorStream = ({ setFps, active, detectorId }) => {
    const [streamSrc, setStreamSrc] = useState(null);
    const canvasContainerRef = useRef();
    const canvasRef = useRef();
    const frameRef = useRef();
    const fpsRef = useRef(0);
    const fpsTimerRef = useRef();
    const isMounted = useMounted();

    useEffect(() => {
        if (active) {
            fpsTimerRef.current = window.setInterval(() => {
                setFps(fpsRef.current);
                fpsRef.current = 0;
            }, 1000);
        } else {
            if (fpsTimerRef.current) window.clearInterval(fpsTimerRef.current);
        }
    }, [active]);

    const drawFrame = () => {
        const canvas = canvasRef.current;
        const canvasContainer = canvasContainerRef.current;

        if (!canvas || !canvasContainer) return;

        // Resize canvas to fill container but keep 4:3 aspect ratio
        const contw = canvasContainer.clientWidth;
        const conth = canvasContainer.clientHeight;
        const ardiff = contw / conth - 4 / 3;

        if (ardiff > 0) {
            canvas.style.width = "auto";
            canvas.style.height = conth + "px";
        } else {
            canvas.style.width = contw + "px";
            canvas.style.height = "auto";
        }

        // if no frame - draw placeholer
        if (!frameRef.current) {
            const ctx = canvas.getContext("2d");
            ctx.drawImage(placeholder, 0, 0);
            return;
        }

        const base64 = frameRef.current;
        const arrayBuffer = decode(base64);

        if (streamSrc) {
            URL.revokeObjectURL(streamSrc);
        }

        const blob = new Blob([arrayBuffer], { type: "image/jpeg" });
        const url = URL.createObjectURL(blob);
        const image = new Image();
        image.src = url;
        image.onload = () => {
            const canvas = canvasRef.current;
            if (!canvas) return;

            const ctx = canvas.getContext("2d");
            ctx.drawImage(image, 0, 0);
            fpsRef.current += 1;
        };

        if (isMounted()) {
            setStreamSrc(url);
        }
    };

    useEffect(() => {
        const canvas = canvasRef.current;
        canvas.width = 640;
        canvas.height = 480;
        drawFrame();
    }, []);

    useEffect(() => {
        if (active) return;

        // NOTE(rg): if we're immediately clearing the canvas when stopping the stream,
        // the last frame will arrive after the canvas is cleared and will overwrite it,
        // hence the timeout
        window.setTimeout(() => {
            const canvas = canvasRef.current;
            const ctx = canvas.getContext("2d");

            ctx.drawImage(placeholder, 0, 0);
        }, 500);
    }, [active]);

    const receiveStreamFrame = (frame) => {
        if (active) {
            frameRef.current = frame;
            drawFrame();
        }
    };

    const receiveSnapshot = () => {};

    const handlers = useMemo(() => {
        return [
            ["StreamFrame", receiveStreamFrame],
            ["Snapshot", receiveSnapshot],
        ];
    });

    useSignalR({
        url: Routes.detectorHub,
        active: active,
        groups: ["Stream-" + detectorId, "Snapshot-" + detectorId],
        handlers: handlers,
    });

    useResizeObserver(canvasContainerRef, (_) => {
        drawFrame();
    });

    return (
        <Box
            ref={canvasContainerRef}
            display="flex"
            justifyContent="center"
            alignItems="center"
            sx={{
                backgroundColor: "black",
                height: "100%",
            }}
        >
            <Box
                component="canvas"
                ref={canvasRef}
                sx={{ maxHeight: "100%", maxWidth: "100%", aspectRatio: "4/3" }}
            />
        </Box>
    );
};

export default DetectorStream;
