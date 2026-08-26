import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import FormHelperText from '@mui/material/FormHelperText';
import type { CatalogItem } from '../services/vehicleService';

interface Props {
  vehicleTypes: CatalogItem[];
  value: string;
  loading: boolean;
  disabled: boolean;
  onChange: (vehicleType: string) => void;
}

/** A handful of options per make, so a plain select is the right control here. */
export function VehicleTypeSelect({ vehicleTypes, value, loading, disabled, onChange }: Props) {
  const empty = !loading && vehicleTypes.length === 0;

  return (
    <FormControl fullWidth disabled={disabled || loading || empty}>
      <InputLabel id="vehicle-type-label">Vehicle type</InputLabel>
      <Select
        labelId="vehicle-type-label"
        label="Vehicle type"
        value={vehicleTypes.some((t) => t.name === value) ? value : ''}
        onChange={(event) => onChange(event.target.value)}
      >
        <MenuItem value="">
          <em>All types</em>
        </MenuItem>
        {vehicleTypes.map((type) => (
          <MenuItem key={type.id} value={type.name}>
            {type.name}
          </MenuItem>
        ))}
      </Select>
      <FormHelperText>
        {disabled
          ? 'Choose a make first'
          : loading
            ? 'Loading types…'
            : empty
              ? 'No types recorded for this make'
              : 'Optional'}
      </FormHelperText>
    </FormControl>
  );
}
