
import os
import matplotlib.pyplot as plt

if __name__ == "__main__":
    root_folder = os.path.dirname(__file__)
    file_lines = {}
    file_comments = {}
    line_count = 0
    comment_count = 0
    file_count = 0
    for root, folders, files in os.walk(root_folder):
        for file in files:
            if file.endswith(".cs"):
                file_count += 1
                file_path = os.path.join(root, file)
                file_lines[file_path] = 0
                file_comments[file_path] = 0
                with open(file_path, "r") as f:
                    for line in f.readlines():
                        if (not line.isspace() and len(line) > 0):
                            if not line.startswith("//"):
                                line_count += 1
                                file_lines[file_path] += 1
                                if ("//" in line):
                                    comment_count += 1
                                    file_comments[file_path] += 1
                            else:
                                comment_count += 1
                                file_comments[file_path] += 1
    print(root_folder, " contains ", file_count, " .cs files.")
    print(root_folder, " contains ~", line_count, " lines of code.")
    print(root_folder, " contains ~", comment_count, " comments.")

    maxlen = 5 + max([len(k) for k in file_lines.keys()])
    file_lines_list = [(f, file_lines[f], file_comments[f]) for f in file_lines.keys()]
    file_lines_list.sort(reverse=True, key=lambda x: x[1])
    # Print list of files sorted by line count
    # for f in file_lines.keys():
    for fl in file_lines_list:
        s = "\t" + fl[0]
        q = maxlen-len(fl[0])
        # l = "{} {:>{q}} lines. {:>4} comments".format(s, file_lines[f], file_comments[f], q=q)
        l = "{} {:>{q}} lines. {:>4} comments".format(s, fl[1], fl[2], q=q)
        print(l)

    plt.hist([k for k in file_lines.values()], bins=20, rwidth=0.9, stacked=True)
    plt.gca().set_xlabel("# Lines of Code")
    plt.gca().set_ylabel("# Files")
    plt.show()