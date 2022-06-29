import { bindPopover } from "material-ui-popup-state/hooks";
import React, { useState, useEffect } from "react";

import CheckIcon from "@mui/icons-material/Check";
import { Popover, TextField } from "@mui/material";
import IconButton from "@mui/material/IconButton";

// TODO(rg): value resets to "" if edit was not successful
const EditPopup = ({ popupProps, initialValue, label, handler }) => {
    const [input, setInput] = useState("");

    useEffect(() => {
        setInput(initialValue);
    }, [initialValue]);

    const handleSubmit = async (e) => {
        e.stopPropagation();

        if (e.key && e.key !== "Enter") return;
        if (!input) return;

        const ok = await handler(input);

        if (ok) {
            setInput("");
            popupProps.close();
        }
    };

    return (
        <Popover
            {...bindPopover(popupProps)}
            anchorOrigin={{
                vertical: "top",
                horizontal: "right",
            }}
            transformOrigin={{
                vertical: "bottom",
                horizontal: "left",
            }}
        >
            <TextField
                label={label}
                variant="filled"
                autoFocus
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyPress={handleSubmit}
                onClick={(e) => e.stopPropagation()}
            />
            <IconButton sx={{ color: "success.light" }} disabled={!input} onClick={handleSubmit}>
                <CheckIcon />
            </IconButton>
        </Popover>
    );
};

export default EditPopup;
