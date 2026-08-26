import Autocomplete from '@mui/material/Autocomplete';
import TextField from '@mui/material/TextField';
import CircularProgress from '@mui/material/CircularProgress';
import InputAdornment from '@mui/material/InputAdornment';
import type { CatalogItem } from '../services/vehicleService';

interface Props {
  makes: CatalogItem[];
  value: CatalogItem | null;
  loading: boolean;
  onChange: (make: CatalogItem | null) => void;
}

/**
 * vPIC lists thousands of makes, so this is a searchable field rather than a plain
 * select — scrolling that list to find "Honda" is not a real interaction.
 */
export function MakeAutocomplete({ makes, value, loading, onChange }: Props) {
  return (
    <Autocomplete
      options={makes}
      value={value}
      loading={loading}
      onChange={(_, make) => onChange(make)}
      getOptionLabel={(make) => make.name}
      isOptionEqualToValue={(a, b) => a.id === b.id}
      // The list is long enough that rendering every option would stutter; MUI only
      // renders what is filtered, and this keeps the filtered set sane too.
      filterOptions={(options, { inputValue }) => {
        const needle = inputValue.trim().toLowerCase();
        if (!needle) return options.slice(0, 100);
        return options.filter((o) => o.name.toLowerCase().includes(needle)).slice(0, 100);
      }}
      renderInput={(params) => (
        <TextField
          {...params}
          label="Make"
          placeholder="Search makes"
          slotProps={{
            input: {
              ...params.InputProps,
              endAdornment: (
                <>
                  {loading ? (
                    <InputAdornment position="end">
                      <CircularProgress size={18} />
                    </InputAdornment>
                  ) : null}
                  {params.InputProps.endAdornment}
                </>
              ),
            },
          }}
        />
      )}
    />
  );
}
