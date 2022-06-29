import React, {useState} from "react";
import {
    Box,
    IconButton, Tooltip,
} from "@mui/material";
import {bindTrigger, usePopupState} from "material-ui-popup-state/hooks";
import AddIcon from '@mui/icons-material/Add';
import axios from "axios";
import LocationCard from "./LocationCard";
import {Routes} from "../../routes";
import EditPopup from "../../common/EditPopup";

const Locations = ({locations, setLocations}) => {
    const addPopup = usePopupState({variant: "popover", popupId: "add-location"});
    const [newLocationName, setNewLocationName] = useState("");

    const onRename = (id, newName) => {
        const data = {name: newName};
        axios.put(`${Routes.locations}/${id}`, data)
            .then(_ => {
                const newLocations = [...locations];
                let editedLocation = newLocations.find(l => l.id === id);
                editedLocation.name = newName;

                setLocations(newLocations);
            });
    }

    const onDetach = (detectorId) => {
        axios.post(`${Routes.detectors}/${detectorId}/detach`)
            .then(_ => {
                const newLocations = [...locations];
                let editedLocation = newLocations.find(l => l.detector?.id === detectorId);
                editedLocation.detector = null;

                setLocations(newLocations);
            });
    }

    const onAttach = (id, detector) => {
        const data = {locationId: id};
        axios.post(`${Routes.detectors}/${detector.id}/attach`, data)
            .then(_ => {
                const newLocations = [...locations];
                let editedLocation = newLocations.find(l => l.id === id);
                editedLocation.detector = detector;

                setLocations(newLocations);
            });
    }

    const onDelete = (id) => {
        axios.delete(`${Routes.locations}/${id}`)
            .then(_ => {
                setLocations(ls => ls.filter(l => l.id !== id));
            });
    }

    const onAdd = (name) => {
        const data = {name: name};
        return axios.post(`${Routes.locations}`, data)
            .then(res => {
                setLocations(l => [...l, res.data]);
                return true;
            })
            .catch(_ => false);
    }

    return (
        <>
            <Box sx={{flexGrow: 1, height: 0}}>
                <Box sx={{
                    height: '100%',
                    bgcolor: '#f6f6f6',
                    overflowY: 'auto'
                }}>
                    {locations.map(l => (
                        <LocationCard
                            key={l.id}
                            location={l}
                            onAttach={onAttach}
                            onDetach={onDetach}
                            onDelete={onDelete}
                            onRename={onRename}
                        />
                    ))}
                    <Box display="flex" justifyContent="center">
                        <Tooltip title="Add new location">
                            <IconButton
                                size="large"
                                color="info"
                                {...bindTrigger(addPopup)}
                            >
                                <AddIcon/>
                            </IconButton>
                        </Tooltip>
                    </Box>
                </Box>
            </Box>
            <EditPopup
                popupProps={addPopup}
                initialValue=""
                label="New location name"
                handler={onAdd}
            />
        </>
    );
}

export default Locations;