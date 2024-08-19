import os
import re

def update_cshtml_files(base_dir):
    for file in os.listdir(base_dir):
        if file.endswith('.cshtml') and not file.startswith('_'):
            file_path = os.path.join(base_dir, file)
            try:
                with open(file_path, 'r+', encoding='utf-8') as f:
                    content = f.read()
                    
                    # Debug: Print the first 500 characters of the file content
                    print(f"Processing {file_path}:")
                    print(content[:500])
                    
                    # Regex to find the value between @model and Model
                    match = re.search(r'@model\s+(\w+)Model', content)
                    if match:
                        value = match.group(1)  # Extract the value before 'Model'
                        new_page_line = f'@page "/{value}"\n'
                        
                        # Debug: Print the extracted value and new page line
                        print(f"Found value: {value}")
                        print(f"New page line: {new_page_line}")

                        # Remove all occurrences of @page except the first
                        modified_content = re.sub(r'(@page\s*){2,}', '', content)
                        
                        # Prepend the new @page line
                        new_content = new_page_line + modified_content
                        
                        # Write the modified content back to the file
                        f.seek(0)
                        f.write(new_content)
                        f.truncate()
                        print(f"Updated: {file_path}")
                    else:
                        print(f"No match found in {file_path}")
            except Exception as e:
                print(f"Error processing {file_path}: {e}")

if __name__ == "__main__":
    # Run the script in the current directory
    base_directory = os.getcwd()
    update_cshtml_files(base_directory)
