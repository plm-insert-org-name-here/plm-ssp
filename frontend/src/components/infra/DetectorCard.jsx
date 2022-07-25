import axios from "axios";
import { usePopupState } from "material-ui-popup-state/hooks";
import React, { useRef, useState } from "react";

import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import LocationOffIcon from "@mui/icons-material/LocationOff";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import PowerIcon from "@mui/icons-material/Power";
import PowerOffIcon from "@mui/icons-material/PowerOff";
import { Box, Card, IconButton, Tooltip } from "@mui/material";

import { Routes } from "../../routes";
import OverflowText from "../common/OverflowText";
import ConfirmPopup from "../common/popups/ConfirmPopup";
import EditPopup from "../common/popups/EditPopup";
import MenuPopup from "../common/popups/MenuPopup";

const DetectorCard = ({ detector, onAttach, onDetach, onDelete, onRename }) => {
    const [attachPopupItems, setAttachPopupItems] = useState([]);

    const renamePopup = usePopupState({ variant: "popover", popupId: "rename-detector" });
    const detachPopup = usePopupState({ variant: "popover", popupId: "detach-detector" });
    const attachPopup = usePopupState({ variant: "popover", popupId: "attach-detector" });
    const deletePopup = usePopupState({ variant: "popover", popupId: "delete-detector" });
    const ref = useRef(null);

    renamePopup.setAnchorEl(ref.current);
    detachPopup.setAnchorEl(ref.current);
    attachPopup.setAnchorEl(ref.current);
    deletePopup.setAnchorEl(ref.current);

    const onRenameInner = (newName) => {
        if (detector.name !== newName) {
            onRename(detector.id, newName);
            return true;
        }

        return false;
    };

    const onDetachInner = () => {
        onDetach(detector.id);
        detachPopup.close();
    };

    const onAttachPopupOpen = () => {
        axios(`${Routes.locations}/free`).then((res) => {
            setAttachPopupItems(res.data);
            attachPopup.open();
        });
    };

    const onAttachInner = (location) => {
        onAttach(location, detector);
        attachPopup.close();
    };

    const onDeleteInner = () => {
        onDelete(detector.id);
        deletePopup.close();
    };

    return (
        <>
            <Card
                ref={ref}
                elevation={1}
                sx={{
                    display: "flex",
                    ml: 2,
                    mr: 3,
                    my: 1,
                    width: "auto",
                    height: "120px",
                    borderRadius: "8px",
                }}
            >
                <Box
                    height="100%"
                    display="flex"
                    width={0}
                    flexGrow={1}
                    flexDirection="column"
                    justifyContent="space-between"
                    sx={{ p: 1 }}
                >
                    <OverflowText
                        text={detector.name}
                        sx={{
                            color: detector.location ? "text.default" : "text.lighter",
                        }}
                        variant="h6"
                    />
                    <Box display="flex" alignItems="center">
                        {detector.location ? (
                            <LocationOnIcon sx={{ height: "32px", color: "text.lighter" }} />
                        ) : (
                            <LocationOffIcon sx={{ height: "32px", color: "text.lightest" }} />
                        )}
                        <OverflowText
                            color="text.lighter"
                            text={detector.location?.name ?? ""}
                            sx={{ pt: 0.6, pl: 1, fontSize: "14px" }}
                        />
                    </Box>
                </Box>
                <Box
                    display="flex"
                    flexDirection="column"
                    className="hidden-icon"
                    bgcolor="#eeeeee"
                >
                    <Tooltip title="Rename" placement="right">
                        <IconButton
                            size="small"
                            color="secondary"
                            onClick={() => renamePopup.open()}
                        >
                            <EditIcon />
                        </IconButton>
                    </Tooltip>
                    {detector.location ? (
                        <Tooltip title="Detach detector" placement="right">
                            <IconButton
                                size="small"
                                color="info"
                                onClick={() => detachPopup.open()}
                            >
                                <PowerOffIcon />
                            </IconButton>
                        </Tooltip>
                    ) : (
                        <Tooltip title="Attach detector" placement="right">
                            <IconButton size="small" color="info" onClick={onAttachPopupOpen}>
                                <PowerIcon />
                            </IconButton>
                        </Tooltip>
                    )}
                    <Tooltip title="Delete" placement="right" onClick={() => deletePopup.open()}>
                        <IconButton size="small" color="error">
                            <DeleteIcon />
                        </IconButton>
                    </Tooltip>
                </Box>
            </Card>
            <EditPopup
                popupProps={renamePopup}
                initialValue={detector.name}
                label="Detector name"
                handler={onRenameInner}
            />
            <ConfirmPopup
                popupProps={detachPopup}
                handler={onDetachInner}
                text={
                    <>
                        Detaching <i>{detector.name}</i>
                    </>
                }
            />
            <MenuPopup popupProps={attachPopup} items={attachPopupItems} handler={onAttachInner} />
            <ConfirmPopup
                popupProps={deletePopup}
                handler={onDeleteInner}
                text={
                    <>
                        Deleting <i>{detector.name}</i>
                    </>
                }
            />
        </>
    );
};

export default DetectorCard;
