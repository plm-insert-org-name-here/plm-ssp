import React, { useRef, useState } from "react";

import SearchIcon from "@mui/icons-material/Search";
import { FilledInput, InputAdornment } from "@mui/material";
import FormControl from "@mui/material/FormControl";

const SimpleSearch = ({
    handler,
    timeout,
    controlSx = { m: 1, width: "25ch" },
    inputSx = {
        pl: "6px",
        bgcolor: "greys.input",
        borderBottom: "none",
        borderRadius: "8px",
        height: "32px",
    },
}) => {
    const [value, setValue] = useState("");
    const timeoutRef = useRef(-1);

    const handleChange = (e) => {
        setValue(e.target.value);

        if (timeout) {
            if (timeoutRef.current) {
                window.clearTimeout(timeoutRef.current);
            }
            timeoutRef.current = window.setTimeout(() => {
                handler(e.target.value);
            }, timeout);
        } else {
            handler(e.target.value);
        }
    };

    return (
        <FormControl hiddenLabel sx={controlSx} variant="filled">
            <FilledInput
                autoFocus
                disableUnderline
                placeholder="Search"
                sx={inputSx}
                value={value}
                onChange={handleChange}
                startAdornment={
                    <InputAdornment position="start">
                        <SearchIcon />
                    </InputAdornment>
                }
            />
        </FormControl>
    );
};

export default SimpleSearch;
