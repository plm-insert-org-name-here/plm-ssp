import useResizeObserver from "@react-hook/resize-observer";
import React, { useRef, useState } from "react";

import { Tooltip, Typography } from "@mui/material";

// NOTE(rg): needs a parent element that sizes itself independently of its children
const OverflowText = ({ text, ...props }) => {
    const [showTooltip, setShowTooltip] = useState(false);
    const textRef = useRef();

    useResizeObserver(textRef, (_) => {
        const elem = textRef.current;
        const shouldShow = elem.scrollWidth > elem.clientWidth;
        setShowTooltip(shouldShow);
    });

    return (
        <Tooltip
            title={text}
            enterDelay={500}
            enterTouchDelay={500}
            disableHoverListener={!showTooltip}
            componentsProps={{
                tooltip: {
                    sx: {
                        bgcolor: "black",
                    },
                },
            }}
        >
            <Typography
                ref={textRef}
                whiteSpace="nowrap"
                overflow="hidden"
                textOverflow="ellipsis"
                {...props}
            >
                {text}
            </Typography>
        </Tooltip>
    );
};

export default OverflowText;
