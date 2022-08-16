import useResizeObserver from "@react-hook/resize-observer";
import React, { useRef, useEffect } from "react";

import Box from "@mui/material/Box";

import placeholderImageUrl from "/public/640x480.jpg";

const DetectorStream = () => {
    const canvasContainerRef = useRef();
    const canvasRef = useRef();

    const img = new Image();
    img.src = placeholderImageUrl;

    useResizeObserver(canvasContainerRef, (_) => {
        const canvas = canvasRef.current;
        const canvasContainer = canvasContainerRef.current;
        draw(canvas, canvasContainer);
    });

    const draw = (canvas, canvasContainer) => {
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
        canvas.width = 640;
        canvas.height = 480;

        const ctx = canvas.getContext("2d");
        ctx.drawImage(img, 0, 0);
    };

    useEffect(() => {
        const canvas = canvasRef.current;
        const canvasContainer = canvasContainerRef.current;

        draw(canvas, canvasContainer);
    }, []);

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
