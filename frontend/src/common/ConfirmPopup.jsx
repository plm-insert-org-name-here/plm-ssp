import { bindPopover } from "material-ui-popup-state/hooks";
import React from "react";

import { Popover } from "@mui/material";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";

const ConfirmPopup = ({ popupProps, handler, text }) => {
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
            <Typography sx={{ m: 2 }}>{text}. Are you sure?</Typography>
            <Box
                sx={{
                    display: "flex",
                    justifyContent: "flex-end",
                    alignItems: "center",
                }}
            >
                <Button variant="text" sx={{ color: "greys.main" }} onClick={popupProps.close}>
                    Cancel
                </Button>
                <Button variant="text" color="error" onClick={handler}>
                    Ok
                </Button>
            </Box>
        </Popover>
    );
};

export default ConfirmPopup;
