import axios from "axios";
import { usePopupState } from "material-ui-popup-state/hooks";
import React, { useRef, useState } from "react";

import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import PowerIcon from "@mui/icons-material/Power";
import PowerOffIcon from "@mui/icons-material/PowerOff";
import VideocamIcon from "@mui/icons-material/Videocam";
import VideocamOffIcon from "@mui/icons-material/VideocamOff";
import { Box, Card, IconButton, Tooltip } from "@mui/material";

import ConfirmPopup from "../../common/ConfirmPopup";
import EditPopup from "../../common/EditPopup";
import MenuPopup from "../../common/MenuPopup";
import OverflowText from "../../common/OverflowText";
import { Routes } from "../../routes";

const LocationCard = ({ location, onAttach, onDetach, onDelete, onRename }) => {
    const [attachPopupItems, setAttachPopupItems] = useState([]);

    const renamePopup = usePopupState({ variant: "popover", popupId: "rename-location" });
    const detachPopup = usePopupState({ variant: "popover", popupId: "detach-detector" });
    const attachPopup = usePopupState({ variant: "popover", popupId: "attach-detector" });
    const deletePopup = usePopupState({ variant: "popover", popupId: "delete-location" });
    const ref = useRef(null);

    renamePopup.setAnchorEl(ref.current);
    detachPopup.setAnchorEl(ref.current);
    attachPopup.setAnchorEl(ref.current);
    deletePopup.setAnchorEl(ref.current);

    const onRenameInner = (newName) => {
        if (location.name !== newName) {
            onRename(location.id, newName);
            return true;
        }

        return false;
    };

    const onDetachInner = () => {
        onDetach(location.detector?.id);
        detachPopup.close();
    };

    const onAttachPopupOpen = () => {
        axios(`${Routes.detectors}/attachable`).then((res) => {
            setAttachPopupItems(res.data);
            attachPopup.open();
        });
    };

    const onAttachInner = (detector) => {
        onAttach(location.id, detector);
        attachPopup.close();
    };

    const onDeleteInner = () => {
        onDelete(location.id);
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
                        text={location.name}
                        sx={{ color: location.detector ? "text.default" : "text.lighter" }}
                        variant="h6"
                    />
                    <Box display="flex" alignItems="center">
                        {location.detector ? (
                            <VideocamIcon sx={{ height: "32px", color: "text.lighter" }} />
                        ) : (
                            <VideocamOffIcon sx={{ height: "32px", color: "text.lightest" }} />
                        )}
                        <OverflowText
                            color="text.lighter"
                            text={location.detector?.name ?? ""}
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
                    {location.detector ? (
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
                initialValue={location.name}
                label="Location name"
                handler={onRenameInner}
            />
            <ConfirmPopup
                popupProps={detachPopup}
                handler={onDetachInner}
                text={
                    <>
                        Detaching <i>{location.detector?.name}</i>
                    </>
                }
            />
            <MenuPopup popupProps={attachPopup} items={attachPopupItems} handler={onAttachInner} />
            <ConfirmPopup
                popupProps={deletePopup}
                handler={onDeleteInner}
                text={
                    <>
                        Deleting <i>{location.name}</i>
                    </>
                }
            />
        </>
    );
};

export default LocationCard;
