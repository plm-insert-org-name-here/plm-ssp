import React, {useEffect, useRef, useState} from 'react';
import {Tooltip, Typography} from "@mui/material";

// NOTE(rg): needs a parent element that sizes itself independently of its children
const OverflowText = ({text, ...props}) => {
    const [showTooltip, setShowTooltip] = useState(false);
    const textRef = useRef();

    useEffect(() => {
        const curr = textRef.current;
        const show = !!(curr && (curr.scrollWidth > curr.clientWidth));
        if (show) setShowTooltip(show);
    }, []);

    const textElement = (
        <Typography
            ref={textRef}
            whiteSpace="nowrap"
            overflow="hidden"
            textOverflow="ellipsis"
            {...props}
        >{text}</Typography>
    );

    return (
        <>
            {
                showTooltip ? (
                    <Tooltip
                        title={text}
                        enterDelay={500}
                        enterTouchDelay={500}
                    >
                        {textElement}
                    </Tooltip>
                ) : (
                    textElement
                )
            }
        </>
    );
}

export default OverflowText;
