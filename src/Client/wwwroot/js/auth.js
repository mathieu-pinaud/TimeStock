window.hashPassword = async function (password) {
    return new Promise((resolve, reject) => {
        dcodeIO.bcrypt.hash(password, 10, function (err, hash) {
            if (err) reject(err);
            else resolve(hash);
        });
    });
};
